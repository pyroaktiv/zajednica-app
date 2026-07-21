using Zajednica.Community.Api.Dto;

namespace Zajednica.Community.Api.Public;

public interface IMembershipService
{
    Task<MembershipDto> GetMineAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<MembershipDto> SetUnitNumberAsync(Guid accountId, Guid communityId, SetUnitNumberRequest request, CancellationToken ct = default);
    Task<MembershipDto> GetAsync(Guid accountId, Guid communityId, Guid membershipId, CancellationToken ct = default);

    Task<IReadOnlyList<CommunityMemberDto>> GetConfirmedAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<IReadOnlyList<CommunityMemberDto>> GetIssuersAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<IReadOnlyList<CommunityMemberDto>> GetUnconfirmedAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<CommunityMemberDto?> GetManagerAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<IReadOnlyList<CommunityMemberDto>> GetRankingAsync(Guid accountId, Guid communityId, CancellationToken ct = default);

    Task GrantIssuerAsync(Guid accountId, Guid communityId, Guid membershipId, CancellationToken ct = default);
}
