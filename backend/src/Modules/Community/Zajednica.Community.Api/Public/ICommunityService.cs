using Zajednica.Community.Api.Dto;

namespace Zajednica.Community.Api.Public;

public interface ICommunityService
{
    Task<CommunityDto> CreateAsync(Guid accountId, CreateCommunityRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CommunitySummaryDto>> GetMineAsync(Guid accountId, CancellationToken ct = default);
    Task<CommunityDto> GetAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<CommunityDto> UpdateAsync(Guid accountId, Guid communityId, UpdateCommunityRequest request, CancellationToken ct = default);
    Task<CommunityQrDto> GetQrAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<MembershipDto> JoinAsync(Guid accountId, JoinCommunityRequest request, CancellationToken ct = default);
    Task LeaveAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
}
