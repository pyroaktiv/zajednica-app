using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Actions;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentService(
    IIntentRepository intents,
    IInternalMembershipAudienceService audience,
    CommunityAccess access,
    IntentAccess lookup,
    IntentClosing closing,
    IntentNotifier notifier,
    IntentPresenter presenter) : IIntentService
{
    public IntentDetailsDto OpenBan(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request)
    {
        var authorMembershipId = access.RequireConfirmed(accountId, communityId);

        return Open(UserActionKind.Ban, communityId, authorMembershipId, request.TargetMembershipId, request.Text);
    }

    public IntentDetailsDto OpenManagerElection(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request)
    {
        var authorMembershipId = access.RequireConfirmed(accountId, communityId);

        return Open(UserActionKind.ManagerElection, communityId, authorMembershipId, request.TargetMembershipId,
            request.Text);
    }

    public IntentDetailsDto Vote(Guid accountId, Guid communityId, Guid intentId, CastVoteRequest request)
    {
        var voterMembershipId = access.RequireConfirmed(accountId, communityId);
        var intent = lookup.RequireAggregate(intentId, communityId);
        var now = DateTime.UtcNow;

        closing.CloseIfDue(intent, now);
        intent.CastVote(voterMembershipId, request.Value, now);

        if (!closing.CloseIfDue(intent, now))
        {
            intents.Update(intent);
            notifier.Changed(intent);
        }

        return presenter.Details(lookup.RequireView(intentId, communityId), request.Value);
    }

    private IntentDetailsDto Open(
        UserActionKind kind, Guid communityId, Guid authorMembershipId, Guid targetMembershipId, string text)
    {
        var context = new IntentContext.Builder()
            .WithCommunityId(communityId)
            .WithAuthorMembershipId(authorMembershipId)
            .WithEligibleVoterCount(audience.GetConfirmedCount(communityId))
            .WithTargetMembershipStatus(access.StatusOf(communityId, targetMembershipId))
            .At(DateTime.UtcNow)
            .Build();

        var intent = Intent.Open(new UserTargetingAction(kind, targetMembershipId, context), text);

        intents.Add(intent);
        notifier.Opened(intent);

        return presenter.Details(lookup.RequireView(intent.Id, communityId), null);
    }
}
