using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases;

public sealed class IntentService(
    IIntentRepository intents,
    IInternalMembershipService memberships,
    CommunityAccess access,
    IntentAccess lookup,
    IntentClosing closing,
    IntentNotifier notifier,
    IntentPresenter presenter) : IIntentService
{
    public IntentDetailsDto OpenBan(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request)
    {
        var author = access.RequireConfirmed(accountId, communityId);
        var target = RequireTarget(request.TargetMembershipId);

        var intent = Intent.Open(
            UserTargetingAction.For(UserActionKind.Ban, target.MembershipId, target.IsConfirmed),
            communityId, author.MembershipId, request.Text, memberships.GetConfirmedCount(communityId),
            DateTime.UtcNow);

        return CompleteOpening(intent, author.MembershipId);
    }

    public IntentDetailsDto OpenManagerElection(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request)
    {
        var author = access.RequireConfirmed(accountId, communityId);
        var target = RequireTarget(request.TargetMembershipId);

        var intent = Intent.Open(
            UserTargetingAction.For(UserActionKind.ManagerElection, target.MembershipId, target.IsConfirmed),
            communityId, author.MembershipId, request.Text, memberships.GetConfirmedCount(communityId),
            DateTime.UtcNow);

        return CompleteOpening(intent, author.MembershipId);
    }

    public IntentDetailsDto Vote(Guid accountId, Guid communityId, Guid intentId, CastVoteRequest request)
    {
        var voter = access.RequireConfirmed(accountId, communityId);
        var intent = lookup.Require(intentId, communityId);
        var now = DateTime.UtcNow;

        if (closing.CloseIfDue(intent, now))
            throw new EntityValidationException("Voting on this intent is closed.");

        intent.CastVote(voter.MembershipId, request.Value, now);

        if (!closing.CloseIfDue(intent, now))
        {
            intents.Update(intent);
            notifier.Changed(intent);
        }

        return presenter.Details(intent, voter.MembershipId);
    }

    private MembershipContextDto RequireTarget(Guid targetMembershipId) =>
        memberships.GetContexts([targetMembershipId]).SingleOrDefault()
        ?? throw new NotFoundException("Membership not found.");

    private IntentDetailsDto CompleteOpening(Intent intent, Guid authorMembershipId)
    {
        intents.Add(intent);
        notifier.Opened(intent);

        return presenter.Details(intent, authorMembershipId);
    }
}
