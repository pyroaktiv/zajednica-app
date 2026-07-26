using Zajednica.Community.Api.Dto.Memberships;

namespace Zajednica.Community.Api.Public;

public interface IMembershipService
{
    MemberProfileDto GetMine(Guid accountId, Guid communityId);
    UnitNumberDto SetUnitNumber(Guid accountId, Guid communityId, SetUnitNumberRequest request);
    MemberProfileDto Get(Guid accountId, Guid communityId, Guid membershipId);

    IReadOnlyList<MemberSummaryDto> GetConfirmed(Guid accountId, Guid communityId);
    IReadOnlyList<MemberSummaryDto> GetIssuers(Guid accountId, Guid communityId);
    IReadOnlyList<MemberSummaryDto> GetUnconfirmed(Guid accountId, Guid communityId);
    MemberSummaryDto? GetManager(Guid accountId, Guid communityId);
    IReadOnlyList<MemberSummaryDto> GetRanking(Guid accountId, Guid communityId);

    void GrantIssuer(Guid accountId, Guid communityId, Guid membershipId);
}
