using Zajednica.Community.Api.Dto.Communities;

namespace Zajednica.Community.Api.Public;

public interface ICommunityService
{
    Task<CommunityDetailsDto> CreateAsync(Guid accountId, CreateCommunityRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<MyCommunityDto>> GetMineAsync(Guid accountId, CancellationToken ct = default);
    Task<CommunityDetailsDto> GetAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<CommunityDetailsDto> UpdateAsync(Guid accountId, Guid communityId, UpdateCommunityRequest request, CancellationToken ct = default);
    Task<CommunityQrDto> GetQrAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<JoinedCommunityDto> JoinAsync(Guid accountId, JoinCommunityRequest request, CancellationToken ct = default);
    Task LeaveAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
}
