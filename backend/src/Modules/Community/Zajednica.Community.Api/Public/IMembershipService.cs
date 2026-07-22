using Zajednica.Community.Api.Dto.Memberships;

namespace Zajednica.Community.Api.Public;

public interface IMembershipService
{
    Task<MyMembershipDto> GetMineAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<UnitNumberDto> SetUnitNumberAsync(Guid accountId, Guid communityId, SetUnitNumberRequest request, CancellationToken ct = default);
    Task<MemberProfileDto> GetAsync(Guid accountId, Guid communityId, Guid membershipId, CancellationToken ct = default);

    Task<IReadOnlyList<MemberSummaryDto>> GetConfirmedAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<IReadOnlyList<MemberSummaryDto>> GetIssuersAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<IReadOnlyList<MemberSummaryDto>> GetUnconfirmedAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<MemberSummaryDto?> GetManagerAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<IReadOnlyList<MemberSummaryDto>> GetRankingAsync(Guid accountId, Guid communityId, CancellationToken ct = default);

    Task GrantIssuerAsync(Guid accountId, Guid communityId, Guid membershipId, CancellationToken ct = default);
}
