using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.Mappers;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.UseCases;

public sealed class IntentService(
    IIntentRepository intents,
    IInternalMembershipService memberships,
    INotificationSender notifications,
    IRealtimePusher realtime,
    AuthorDirectory authors,
    CommunityAccess access) : IIntentService
{
    public UserTargetingIntentDetailsDto OpenBan(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request) =>
        Open(accountId, communityId, IntentKind.Ban, request);

    public UserTargetingIntentDetailsDto OpenManagerElection(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request) =>
        Open(accountId, communityId, IntentKind.ManagerElection, request);

    public UserTargetingIntentDetailsDto Vote(Guid accountId, Guid communityId, Guid intentId, CastVoteRequest request)
    {
        var voter = access.RequireConfirmed(accountId, communityId);
        var intent = Require(intentId, communityId);
        var now = DateTime.UtcNow;

        if (intent.ShouldClose(now))
        {
            Close(intent, now);
            throw new EntityValidationException("Voting on this intent is closed.");
        }

        intent.CastVote(voter.MembershipId, request.Value, now);

        if (intent.ShouldClose(now))
        {
            Close(intent, now);
        }
        else
        {
            intents.Update(intent);
            Push(intent);
        }

        return Details(intent, voter.MembershipId);
    }

    public UserTargetingIntentDetailsDto Get(Guid accountId, Guid communityId, Guid intentId)
    {
        var reader = access.RequireConfirmed(accountId, communityId);
        var intent = Require(intentId, communityId);

        var now = DateTime.UtcNow;
        if (intent.ShouldClose(now))
            Close(intent, now);

        return Details(intent, reader.MembershipId);
    }

    public Page<UserTargetingIntentSummaryDto> GetPage(Guid accountId, Guid communityId, DateTime? before, int limit)
    {
        access.RequireConfirmed(accountId, communityId);
        CloseDue(communityId);

        var page = intents.GetPage(communityId, before, Paging.Clamp(limit));
        var profiles = authors.For(
            page.Items.Where(v => v.TargetMembershipId is not null).Select(v => v.TargetMembershipId!.Value).ToList());

        return new Page<UserTargetingIntentSummaryDto>(
            page.Items.Select(v => v.ToSummaryDto(Profile(profiles, v.TargetMembershipId))).ToList(),
            page.NextCursor);
    }

    private UserTargetingIntentDetailsDto Open(Guid accountId, Guid communityId, IntentKind kind,
        OpenUserTargetingIntentRequest request)
    {
        var author = access.RequireConfirmed(accountId, communityId);

        var target = (memberships.GetContexts([request.TargetMembershipId])).SingleOrDefault()
            ?? throw new NotFoundException("Membership not found.");
        var eligibleVoterCount = memberships.GetConfirmedCount(communityId);
        var targetIsConfirmedMember = target.CommunityId == communityId && target.IsActive && target.IsConfirmed;

        Intent intent = kind == IntentKind.Ban
            ? BanIntent.Open(communityId, author.MembershipId, request.TargetMembershipId, request.Text,
                eligibleVoterCount, targetIsConfirmedMember, DateTime.UtcNow)
            : ManagerElectionIntent.Open(communityId, author.MembershipId, request.TargetMembershipId, request.Text,
                eligibleVoterCount, targetIsConfirmedMember, DateTime.UtcNow);
        intents.Add(intent);

        notifications.Send(new NotificationRequest(target.AccountId, "Pokrenuta namera",
            "U zajednici je pokrenuta namera koja se odnosi na vas.", NotificationPriority.Default));
        realtime.PushToChannel(Channels.Community(communityId),
            new RealtimeMessage("intents.changed", new { communityId }));

        return Details(intent, author.MembershipId);
    }

    private void Close(Intent intent, DateTime now)
    {
        var status = intent.Close(now);
        intents.Update(intent);

        if (status == IntentStatus.Accepted)
            Execute(intent);

        NotifyClosed(intent, status);
        Push(intent);
    }

    private void Execute(Intent intent)
    {
        switch (intent)
        {
            case BanIntent ban:
                memberships.Ban(ban.TargetMembershipId, ban.Id);
                CancelOtherIntents(ban);
                break;

            case ManagerElectionIntent election:
                memberships.ElectManager(election.TargetMembershipId);
                break;
        }
    }

    private void CancelOtherIntents(BanIntent ban)
    {
        var now = DateTime.UtcNow;
        var open = intents.GetOpenViewsByTarget(ban.CommunityId, ban.TargetMembershipId);

        foreach (var view in open.Where(v => v.Id != ban.Id))
        {
            var other = intents.Get(view.Id);
            if (other is null)
                continue;

            other.Cancel(now);
            intents.Update(other);
            Push(other);
        }
    }

    private void CloseDue(Guid communityId)
    {
        var now = DateTime.UtcNow;

        foreach (var view in intents.GetDueViews(communityId, now))
        {
            var intent = intents.Get(view.Id);
            if (intent is null || !intent.ShouldClose(now))
                continue;

            Close(intent, now);
        }
    }

    private Intent Require(Guid intentId, Guid communityId)
    {
        var intent = intents.Get(intentId);
        if (intent is null || intent.CommunityId != communityId)
            throw new NotFoundException("Intent not found in this community.");

        return intent;
    }

    private UserTargetingIntentDetailsDto Details(Intent intent, Guid readerMembershipId)
    {
        var targetMembershipId = TargetOf(intent);
        var lookedUp = targetMembershipId is null
            ? new List<Guid> { intent.AuthorMembershipId }
            : [intent.AuthorMembershipId, targetMembershipId.Value];
        var profiles = authors.For(lookedUp);

        return intent.ToDetailsDto(
            profiles.GetValueOrDefault(intent.AuthorMembershipId),
            targetMembershipId,
            Profile(profiles, targetMembershipId),
            readerMembershipId);
    }

    private void NotifyClosed(Intent intent, IntentStatus status)
    {
        if (TargetOf(intent) is not { } targetMembershipId)
            return;

        var target = (memberships.GetContexts([targetMembershipId])).SingleOrDefault();
        if (target is null)
            return;

        notifications.Send(new NotificationRequest(target.AccountId, "Namera je zaključena",
            $"Namera koja se odnosi na vas je zaključena sa ishodom {status}.",
            status == IntentStatus.Accepted ? NotificationPriority.High : NotificationPriority.Default));
    }

    private static Guid? TargetOf(Intent intent) => (intent as UserTargetingIntent)?.TargetMembershipId;

    private static AccountProfileDto? Profile(IReadOnlyDictionary<Guid, AccountProfileDto> profiles, Guid? membershipId) =>
        membershipId is null ? null : profiles.GetValueOrDefault(membershipId.Value);

    private void Push(Intent intent) =>
        realtime.PushToChannel(Channels.Intent(intent.Id), new RealtimeMessage("intent.updated", new
        {
            id = intent.Id,
            votesFor = intent.VotesFor,
            votesAgainst = intent.VotesAgainst,
            status = intent.Status.ToString()
        }));
}
