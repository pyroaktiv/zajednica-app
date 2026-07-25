using Zajednica.Community.Api.Dto.Communities;

namespace Zajednica.Community.Api.Public;

public interface ICommunityService
{
    CommunityDetailsDto Create(Guid accountId, CreateCommunityRequest request);
    IReadOnlyList<MyCommunityDto> GetMine(Guid accountId);
    CommunityDetailsDto Get(Guid accountId, Guid communityId);
    CommunityDetailsDto Update(Guid accountId, Guid communityId, UpdateCommunityRequest request);
    CommunityQrDto GetQr(Guid accountId, Guid communityId);
    JoinedCommunityDto Join(Guid accountId, JoinCommunityRequest request);
    void Leave(Guid accountId, Guid communityId);
}
