using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentQueryService(
    IIntentQueryStore intentQueryStore,
    MemberRequirementsService requirementsService,
    IntentRetrievalService retrievalService,
    IntentPresenterService presenterService) : IIntentQueryService
{
    public IntentDetailsDto Get(Guid accountId, Guid communityId, Guid intentId)
    {
        var readerMembershipId = requirementsService.RequireConfirmed(accountId, communityId);
        var view = retrievalService.RequireView(intentId, communityId);

        return presenterService.Details(view, intentQueryStore.GetVote(intentId, readerMembershipId));
    }

    public IReadOnlyList<IntentVoterDto> GetVotes(Guid accountId, Guid communityId, Guid intentId)
    {
        requirementsService.RequireConfirmed(accountId, communityId);
        var view = retrievalService.RequireView(intentId, communityId);

        if (!view.VotesAreVisible)
            throw new ForbiddenException("Votes on this intent are not open to members.");

        return presenterService.Voters(intentQueryStore.GetVotes(intentId));
    }

    public CursorPage<IntentSummaryDto, PageCursor> GetPage(Guid accountId, Guid communityId, PageCursor? before, int limit)
    {
        requirementsService.RequireConfirmed(accountId, communityId);

        return presenterService.Summaries(intentQueryStore.GetPage(communityId, before, Paging.Clamp(limit)));
    }
}
