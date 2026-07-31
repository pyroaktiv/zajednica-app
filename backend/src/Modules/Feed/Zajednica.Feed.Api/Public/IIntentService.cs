using Zajednica.Feed.Api.Dto.Intents;

namespace Zajednica.Feed.Api.Public;

public interface IIntentService
{
    IntentDetailsDto OpenBan(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request);
    IntentDetailsDto OpenManagerElection(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request);
    IntentDetailsDto OpenMute(Guid accountId, Guid communityId, OpenUserTargetingIntentRequest request);
    IntentDetailsDto Vote(Guid accountId, Guid communityId, Guid intentId, CastVoteRequest request);
}
