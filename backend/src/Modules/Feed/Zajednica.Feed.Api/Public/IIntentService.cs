using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Intents;

namespace Zajednica.Feed.Api.Public;

public interface IIntentService
{
    UserTargetingIntentDetailsDto OpenBan(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request);
    UserTargetingIntentDetailsDto OpenManagerElection(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request);
    UserTargetingIntentDetailsDto Vote(Guid accountId, Guid communityId, Guid intentId, CastVoteRequest request);

    UserTargetingIntentDetailsDto Get(Guid accountId, Guid communityId, Guid intentId);
    Page<UserTargetingIntentSummaryDto> GetPage(Guid accountId, Guid communityId, DateTime? before, int limit);
}
