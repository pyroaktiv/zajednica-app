using Zajednica.Feed.Api.Dto.Intents;

namespace Zajednica.Feed.Api.Public;

public interface IIntentCommandService
{
    IntentDetailsDto OpenBan(Guid accountId, Guid communityId, OpenUserTargetingIntentRequestDto requestDto);
    IntentDetailsDto OpenManagerElection(Guid accountId, Guid communityId, OpenUserTargetingIntentRequestDto requestDto);
    IntentDetailsDto OpenMute(Guid accountId, Guid communityId, OpenUserTargetingIntentRequestDto requestDto);
    IntentDetailsDto Vote(Guid accountId, Guid communityId, Guid intentId, CastVoteRequestDto requestDto);
}
