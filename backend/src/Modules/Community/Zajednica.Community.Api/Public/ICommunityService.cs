using Zajednica.Community.Api.Dto.Communities;

namespace Zajednica.Community.Api.Public;

public interface ICommunityService
{
    CommunityDetailsDto Create(Guid accountId, CreateCommunityRequestDto requestDto);
    IReadOnlyList<MyCommunityDto> GetMine(Guid accountId);
    CommunityDetailsDto Get(Guid accountId, Guid communityId);
    CommunityDetailsDto Update(Guid accountId, Guid communityId, UpdateCommunityRequestDto requestDto);
    CommunityQrDto GetQr(Guid accountId, Guid communityId);
    JoinedCommunityDto Join(Guid accountId, JoinCommunityRequestDto requestDto);
    void Leave(Guid accountId, Guid communityId);
}
