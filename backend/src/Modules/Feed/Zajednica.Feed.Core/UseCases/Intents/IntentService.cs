using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentService(
    IIntentRepository intents,
    IInternalMembershipService memberships,
    MemberDirectory directory,
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

        return Open(UserActionKind.Ban, communityId, author.MembershipId, target, request.Text);
    }

    public IntentDetailsDto OpenManagerElection(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request)
    {
        var author = access.RequireConfirmed(accountId, communityId);
        var target = RequireTarget(request.TargetMembershipId);

        return Open(UserActionKind.ManagerElection, communityId, author.MembershipId, target, request.Text);
    }

    public IntentDetailsDto Vote(Guid accountId, Guid communityId, Guid intentId, CastVoteRequest request)
    {
        var voter = access.RequireConfirmed(accountId, communityId);
        var intent = lookup.RequireAggregate(intentId, communityId);
        var now = DateTime.UtcNow;

        closing.CloseIfDue(intent, now);
        intent.CastVote(voter.MembershipId, request.Value, now);

        if (!closing.CloseIfDue(intent, now))
        {
            intents.Update(intent);
            notifier.Changed(intent);
        }

        return presenter.Details(lookup.RequireView(intentId, communityId), request.Value);
    }

    private IntentDetailsDto Open(
        UserActionKind kind, Guid communityId, Guid authorMembershipId, MembershipContextDto target, string text)
    {
        var intent = Intent.Open(
            UserTargetingAction.For(kind, target.MembershipId, target.IsConfirmed),
            communityId, authorMembershipId, text, memberships.GetConfirmedCount(communityId),
            DateTime.UtcNow);

        intents.Add(intent);
        notifier.Opened(intent);

        return presenter.Details(lookup.RequireView(intent.Id, communityId), null);
    }

    private MembershipContextDto RequireTarget(Guid targetMembershipId) =>
        directory.Context(targetMembershipId)
        ?? throw new NotFoundException("Membership not found.");
}
