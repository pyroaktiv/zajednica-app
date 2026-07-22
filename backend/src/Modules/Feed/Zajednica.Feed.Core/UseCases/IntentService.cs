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

namespace Zajednica.Feed.Core.UseCases;

public sealed class IntentService(
    IIntentRepository intents,
    IInternalMembershipService memberships,
    INotificationSender notifications,
    IRealtimePusher realtime,
    AuthorDirectory authors,
    CommunityAccess access) : IIntentService
{
    private delegate Intent OpenIntent(Guid communityId, Guid authorMembershipId, Guid targetMembershipId, string text,
        int eligibleVoterCount, bool targetEligible, DateTime now);

    public Task<IntentDetailsDto> OpenBanAsync(Guid accountId, Guid communityId, OpenIntentRequest request,
        CancellationToken ct = default) =>
        OpenAsync(accountId, communityId, request, BanIntent.Open, ct);

    public Task<IntentDetailsDto> OpenManagerElectionAsync(Guid accountId, Guid communityId, OpenIntentRequest request,
        CancellationToken ct = default) =>
        OpenAsync(accountId, communityId, request, ManagerElectionIntent.Open, ct);

    public async Task<IntentDetailsDto> VoteAsync(Guid accountId, Guid communityId, Guid intentId, CastVoteRequest request,
        CancellationToken ct = default)
    {
        var voter = await access.RequireConfirmedAsync(accountId, communityId, ct);
        var intent = await RequireAsync(intentId, communityId, ct);
        var now = DateTime.UtcNow;

        if (intent.ShouldClose(now))
        {
            await CloseAsync(intent, now, ct);
            throw new EntityValidationException("Voting on this intent is closed.");
        }

        intent.CastVote(voter.MembershipId, request.Value, now);

        if (intent.ShouldClose(now))
        {
            await CloseAsync(intent, now, ct);
        }
        else
        {
            await intents.UpdateAsync(intent, ct);
            await PushAsync(intent, ct);
        }

        return await DetailsAsync(intent, voter.MembershipId, ct);
    }

    public async Task<IntentDetailsDto> GetAsync(Guid accountId, Guid communityId, Guid intentId, CancellationToken ct = default)
    {
        var reader = await access.RequireConfirmedAsync(accountId, communityId, ct);
        var intent = await RequireAsync(intentId, communityId, ct);

        var now = DateTime.UtcNow;
        if (intent.ShouldClose(now))
            await CloseAsync(intent, now, ct);

        return await DetailsAsync(intent, reader.MembershipId, ct);
    }

    public async Task<PagedResult<IntentSummaryDto>> GetPagedAsync(Guid accountId, Guid communityId, int page, int pageSize,
        CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);
        await CloseDueAsync(communityId, ct);

        var paged = await intents.GetPagedViewsAsync(communityId, page, pageSize, ct);
        var profiles = await authors.ForAsync(paged.Results.Select(v => v.TargetMembershipId).ToList(), ct);

        return new PagedResult<IntentSummaryDto>(
            paged.Results.Select(v => v.ToSummaryDto(profiles.GetValueOrDefault(v.TargetMembershipId))).ToList(),
            paged.TotalCount);
    }

    private async Task<IntentDetailsDto> OpenAsync(Guid accountId, Guid communityId, OpenIntentRequest request,
        OpenIntent open, CancellationToken ct)
    {
        var author = await access.RequireConfirmedAsync(accountId, communityId, ct);

        var target = (await memberships.GetContextsAsync([request.TargetMembershipId], ct)).SingleOrDefault()
            ?? throw new NotFoundException("Membership not found.");
        var eligibleVoterCount = await memberships.GetConfirmedCountAsync(communityId, ct);
        var targetEligible = target.CommunityId == communityId && target.IsActive && target.IsConfirmed;

        var intent = open(communityId, author.MembershipId, request.TargetMembershipId, request.Text,
            eligibleVoterCount, targetEligible, DateTime.UtcNow);
        await intents.AddAsync(intent, ct);

        await notifications.SendAsync(new NotificationRequest(target.AccountId, "Pokrenuta namera",
            "U zajednici je pokrenuta namera koja se odnosi na vas.", NotificationPriority.Default), ct);
        await realtime.PushToChannelAsync(Channels.Community(communityId),
            new RealtimeMessage("intents.changed", new { communityId }), ct);

        return await DetailsAsync(intent, author.MembershipId, ct);
    }

    private async Task CloseAsync(Intent intent, DateTime now, CancellationToken ct)
    {
        var outcome = intent.Close(now);
        await intents.UpdateAsync(intent, ct);

        if (outcome.Accepted)
            await ExecuteAsync(intent, ct);

        await NotifyClosedAsync(intent, outcome, ct);
        await PushAsync(intent, ct);
    }

    private async Task ExecuteAsync(Intent intent, CancellationToken ct)
    {
        switch (intent)
        {
            case BanIntent ban:
                await memberships.BanAsync(ban.TargetMembershipId, ban.Id, ct);
                await CancelOtherIntentsAsync(ban, ct);
                break;
            case ManagerElectionIntent election:
                await memberships.ElectManagerAsync(election.TargetMembershipId, ct);
                break;
        }
    }

    private async Task CancelOtherIntentsAsync(BanIntent ban, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var open = await intents.GetOpenViewsByTargetAsync(ban.CommunityId, ban.TargetMembershipId, ct);

        foreach (var view in open.Where(v => v.Id != ban.Id))
        {
            var other = await intents.GetAsync(view.Id, ct);
            if (other is null)
                continue;

            other.Cancel(now);
            await intents.UpdateAsync(other, ct);
            await PushAsync(other, ct);
        }
    }

    private async Task CloseDueAsync(Guid communityId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        foreach (var view in await intents.GetDueViewsAsync(communityId, now, ct))
        {
            var intent = await intents.GetAsync(view.Id, ct);
            if (intent is null || !intent.ShouldClose(now))
                continue;

            await CloseAsync(intent, now, ct);
        }
    }

    private async Task<Intent> RequireAsync(Guid intentId, Guid communityId, CancellationToken ct)
    {
        var intent = await intents.GetAsync(intentId, ct);
        if (intent is null || intent.CommunityId != communityId)
            throw new NotFoundException("Intent not found in this community.");

        return intent;
    }

    private async Task<IntentDetailsDto> DetailsAsync(Intent intent, Guid readerMembershipId, CancellationToken ct)
    {
        var profiles = await authors.ForAsync([intent.AuthorMembershipId, intent.TargetMembershipId], ct);

        return intent.ToDetailsDto(
            profiles.GetValueOrDefault(intent.AuthorMembershipId),
            profiles.GetValueOrDefault(intent.TargetMembershipId),
            readerMembershipId);
    }

    private async Task NotifyClosedAsync(Intent intent, IntentOutcome outcome, CancellationToken ct)
    {
        var target = (await memberships.GetContextsAsync([intent.TargetMembershipId], ct)).SingleOrDefault();
        if (target is null)
            return;

        await notifications.SendAsync(new NotificationRequest(target.AccountId, "Namera je zaključena",
            $"Namera koja se odnosi na vas je zaključena sa ishodom {outcome.Status}.",
            outcome.Accepted ? NotificationPriority.High : NotificationPriority.Default), ct);
    }

    private Task PushAsync(Intent intent, CancellationToken ct) =>
        realtime.PushToChannelAsync(Channels.Intent(intent.Id), new RealtimeMessage("intent.updated", new
        {
            id = intent.Id,
            votesFor = intent.VotesFor,
            votesAgainst = intent.VotesAgainst,
            status = intent.Status.ToString()
        }), ct);
}
