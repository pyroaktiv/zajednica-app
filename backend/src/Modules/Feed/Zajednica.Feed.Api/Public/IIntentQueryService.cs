using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Intents;

namespace Zajednica.Feed.Api.Public;

public interface IIntentQueryService
{
    IntentDetailsDto Get(Guid accountId, Guid communityId, Guid intentId);
    IReadOnlyList<IntentVoterDto> GetVotes(Guid accountId, Guid communityId, Guid intentId);
    CursorPage<IntentSummaryDto> GetPage(Guid accountId, Guid communityId, DateTime? before, int limit);
}
