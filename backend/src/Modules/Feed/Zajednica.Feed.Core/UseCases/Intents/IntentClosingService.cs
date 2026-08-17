using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentClosingService(
    IIntentRepository intentRepository,
    IPostRepository postRepository,
    IIntentQueryStore intentQueryStore,
    IFeedUnitOfWork unitOfWork,
    IInternalMembershipCommandService internalCommandService,
    IntentNotifier notifier)
{
    private const int MaxAttempts = 5;

    public bool CloseIfDue(Intent intent, DateTime now)
    {
        if (StageForClosing(intent, now) is not { } outcome)
            return false;

        unitOfWork.Save();
        ApplyConsequences(outcome);

        return true;
    }

    public void CloseDue()
    {
        var now = DateTime.UtcNow;

        foreach (var intentId in intentQueryStore.GetDueIds(now))
            CloseOne(intentId, now);
    }

    private void CloseOne(Guid intentId, DateTime now)
    {
        for (var attempt = 1; ; attempt++)
        {
            unitOfWork.BeginTransaction();
            try
            {
                if (StageForClosing(intentRepository.Load(intentId), now) is not { } outcome)
                {
                    unitOfWork.Rollback();
                    return;
                }

                unitOfWork.Save();
                unitOfWork.Commit();
                ApplyConsequences(outcome);

                return;
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                unitOfWork.Rollback();
            }
        }
    }

    private StagedOutcome? StageForClosing(Intent? intent, DateTime now)
    {
        if (intent is null || !intent.ShouldClose(now))
            return null;

        var status = intent.Close(now);
        intentRepository.Update(intent);

        var superseded = status == IntentStatus.Accepted ? SupersedeConflicting(intent, now) : [];

        return new StagedOutcome(intent, status, superseded);
    }

    private IReadOnlyList<Intent> SupersedeConflicting(Intent decided, DateTime now)
    {
        var conflicting = intentRepository
            .LoadOpenInCommunity(decided.Initiative.CommunityId)
            .Where(other => other.Id != decided.Id && decided.Initiative.Supersedes(other.Initiative))
            .ToList();

        foreach (var other in conflicting)
        {
            other.Supersede(now);
            intentRepository.Update(other);
        }

        return conflicting;
    }

    private void ApplyConsequences(StagedOutcome outcome)
    {
        Execute(outcome.Decided, outcome.Status);

        notifier.Closed(outcome.Decided, outcome.Status);

        foreach (var other in outcome.Superseded)
            notifier.Changed(other);
    }
    
    private void Execute(Intent intent, IntentStatus status)
    {
        switch (intent.Initiative)
        {
            case UserTargetingInitiative { Kind: UserActionKind.Ban } ban when status == IntentStatus.Accepted:
                internalCommandService.Ban(ban.TargetMembershipId, intent.Id);
                break;

            case UserTargetingInitiative { Kind: UserActionKind.ManagerElection } election when status == IntentStatus.Accepted:
                internalCommandService.ElectManager(election.TargetMembershipId);
                break;

            case UserTargetingInitiative { Kind: UserActionKind.Mute } mute when status == IntentStatus.Accepted:
                internalCommandService.Mute(mute.TargetMembershipId);
                break;

            case PostTargetingInitiative post when status is IntentStatus.Accepted or IntentStatus.Rejected:
                RatePost(post, intent.Id, status == IntentStatus.Accepted, intent.VotesFor, intent.VotesAgainst);
                break;
        }
    }

    private void RatePost(PostTargetingInitiative initiative, Guid intentId, bool approved, int votesFor, int votesAgainst)
    {
        if (postRepository.Get(initiative.PostId) is not GeneralTopicPost post)
            return;

        post.RateByCommunity(new CommunityRating(intentId, approved, votesFor, votesAgainst));
        unitOfWork.Save();
    }

    private sealed record StagedOutcome(Intent Decided, IntentStatus Status, IReadOnlyList<Intent> Superseded);
}
