using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentQueryService(
    IIntentQueryStore intentQueries,
    CommunityAccess access,
    IntentAccess lookup,
    IntentPresenter presenter) : IIntentQueryService
{
    public IntentDetailsDto Get(Guid accountId, Guid communityId, Guid intentId)
    {
        var reader = access.RequireConfirmed(accountId, communityId);
        var view = lookup.RequireView(intentId, communityId);

        return presenter.Details(view, intentQueries.GetVote(intentId, reader.MembershipId));
    }

    public IReadOnlyList<IntentVoterDto> GetVotes(Guid accountId, Guid communityId, Guid intentId)
    {
        access.RequireConfirmed(accountId, communityId);
        lookup.RequireView(intentId, communityId);

        return presenter.Voters(intentQueries.GetVotes(intentId));
    }

    public CursorPage<IntentSummaryDto> GetPage(Guid accountId, Guid communityId, DateTime? before, int limit)
    {
        access.RequireConfirmed(accountId, communityId);

        return presenter.Summaries(intentQueries.GetPage(communityId, before, Paging.Clamp(limit)));
    }
}
