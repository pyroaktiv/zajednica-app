using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Intents;

namespace Zajednica.Feed.Api.Public;

public interface IIntentService
{
    Task<IntentDetailsDto> OpenBanAsync(Guid accountId, Guid communityId, OpenIntentRequest request, CancellationToken ct = default);
    Task<IntentDetailsDto> OpenManagerElectionAsync(Guid accountId, Guid communityId, OpenIntentRequest request, CancellationToken ct = default);
    Task<IntentDetailsDto> VoteAsync(Guid accountId, Guid communityId, Guid intentId, CastVoteRequest request, CancellationToken ct = default);

    Task<IntentDetailsDto> GetAsync(Guid accountId, Guid communityId, Guid intentId, CancellationToken ct = default);
    Task<PagedResult<IntentSummaryDto>> GetPagedAsync(Guid accountId, Guid communityId, int page, int pageSize, CancellationToken ct = default);
}
