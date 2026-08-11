using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentCommandService(
    IIntentRepository intentRepository,
    IFeedUnitOfWork unitOfWork,
    IInternalMembershipAudienceService internalAudienceService,
    MemberRequirementsService requirementsService,
    IntentRetrievalService retrievalService,
    IntentClosingService closingService,
    IntentNotifier notifier,
    IntentPresenterService presenterService) : IIntentCommandService
{
    public IntentDetailsDto OpenBan(Guid accountId, Guid communityId, OpenUserTargetingIntentRequestDto requestDto)
    {
        var authorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        return Open(UserActionKind.Ban, communityId, authorMembershipId, requestDto.TargetMembershipId, requestDto.Text);
    }

    public IntentDetailsDto OpenManagerElection(Guid accountId, Guid communityId, OpenUserTargetingIntentRequestDto requestDto)
    {
        var authorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        return Open(UserActionKind.ManagerElection, communityId, authorMembershipId, requestDto.TargetMembershipId,
            requestDto.Text);
    }

    public IntentDetailsDto OpenMute(Guid accountId, Guid communityId, OpenUserTargetingIntentRequestDto requestDto)
    {
        var authorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        return Open(UserActionKind.Mute, communityId, authorMembershipId, requestDto.TargetMembershipId, requestDto.Text);
    }

    public IntentDetailsDto Vote(Guid accountId, Guid communityId, Guid intentId, CastVoteRequestDto requestDto)
    {
        var voter = requirementsService.RequireVoter(accountId, communityId);
        var intent = retrievalService.RequireAggregate(intentId, communityId);
        var now = DateTime.UtcNow;

        closingService.CloseIfDue(intent, now);
        intent.CastVote(voter, requestDto.Value, now);

        if (!closingService.CloseIfDue(intent, now))
        {
            intentRepository.Update(intent);
            unitOfWork.Save();
            notifier.Changed(intent);
        }

        return presenterService.Details(retrievalService.RequireView(intentId, communityId), requestDto.Value);
    }

    private IntentDetailsDto Open(
        UserActionKind kind, Guid communityId, Guid authorMembershipId, Guid targetMembershipId, string text)
    {
        var initiative = new UserTargetingInitiative(
            kind,
            requirementsService.StandingOf(communityId, targetMembershipId),
            communityId,
            authorMembershipId,
            internalAudienceService.GetConfirmedCount(communityId),
            text);

        var intent = Intent.Open(initiative, DateTime.UtcNow);

        intentRepository.Add(intent);
        unitOfWork.Save();
        notifier.Opened(intent);

        return presenterService.Details(retrievalService.RequireView(intent.Id, communityId), null);
    }
}
