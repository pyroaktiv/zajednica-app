using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Dto.Memberships;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Community.Core.Domain;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Community.Core.Mappers;

public static class MembershipMappers
{
    public static MemberSummaryDto ToSummaryDto(this Membership membership, AccountProfileDto? profile) =>
        new(membership.Id,
            membership.AccountId,
            profile?.Username ?? string.Empty,
            profile?.ImageUrl,
            membership.IsConfirmed(),
            membership.IsConfirmed() ? membership.Stars : null,
            membership.Roles.Select(r => r.Role.ToString()).ToList());

    public static MemberProfileDto ToProfileDto(this Membership membership, AccountProfileDto? profile) =>
        new(membership.Id,
            membership.AccountId,
            profile?.Username ?? string.Empty,
            profile?.ImageUrl,
            profile?.FirstName,
            profile?.LastName,
            profile?.Phone,
            profile?.ContactEmail,
            membership.UnitNumber,
            membership.IsConfirmed(),
            membership.IsConfirmed() ? membership.Stars : null,
            membership.Roles.Select(r => r.Role.ToString()).ToList(),
            membership.DateJoined,
            membership.State.ToString());

    public static JoinedCommunityDto ToJoinedDto(this Membership membership, string communityName) =>
        new(membership.Id,
            membership.CommunityId,
            communityName,
            membership.IsConfirmed());

    public static UnitNumberDto ToUnitNumberDto(this Membership membership) =>
        new(membership.Id, membership.UnitNumber);

    public static CertificationResultDto ToCertificationResultDto(this Membership membership, DateTime certifiedAt) =>
        new(membership.Id,
            membership.CommunityId,
            certifiedAt);

    public static MembershipContextDto ToContextDto(this Membership membership) =>
        new(membership.Id,
            membership.AccountId,
            membership.CommunityId,
            membership.IsConfirmed(),
            membership.IsActive(),
            membership.Roles.Select(r => r.Role.ToString()).ToList());

    public static IReadOnlyList<MemberSummaryDto> ToSummaryDtos(
        this IEnumerable<Membership> memberships, IReadOnlyDictionary<Guid, AccountProfileDto> profiles) =>
        memberships
            .Select(m => m.ToSummaryDto(profiles.GetValueOrDefault(m.AccountId)))
            .ToList();
}
