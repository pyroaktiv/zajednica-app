using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Dto.Memberships;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Community.Core.Domain;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Community.Core.Mappers;

public static class MembershipMappers
{
    public static MemberSummaryDto ToSummaryDto(this Membership membership, InternalProfileDto? profile) =>
        new(membership.Id,
            membership.AccountId,
            profile?.Username ?? string.Empty,
            profile?.ImageUrl,
            membership.IsConfirmed(),
            membership.IsConfirmed() ? membership.Stars : null,
            membership.Roles.Select(r => r.Role.ToString()).ToList());

    public static MemberProfileDto ToProfileDto(this Membership membership, InternalProfileDto? profile,
        DateTime now) =>
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
            membership.State.ToString(),
            membership.IsMuted(now) ? membership.MutedUntil : null);

    public static JoinedCommunityDto ToJoinedDto(this Membership membership, string communityName) =>
        new(membership.Id,
            membership.CommunityId,
            communityName,
            membership.IsConfirmed());

    public static UnitNumberDto ToUnitNumberDto(this Membership membership) =>
        new(membership.Id, membership.UnitNumber);

    public static CertificationResultDto ToCertificationResultDto(this Membership membership) =>
        new(membership.Id,
            membership.CommunityId,
            membership.Certificate!.Date);

    public static InternalMembershipAccountIdDto ToAccountDto(this Membership membership) =>
        new(membership.Id, membership.AccountId);

    public static InternalMembershipFactsDto ToFactsDto(this Membership membership) =>
        new(membership.Id,
            membership.AccountId,
            membership.CommunityId,
            membership.IsActive(),
            membership.IsConfirmed(),
            membership.State == MembershipState.Banned,
            membership.MutedUntil,
            membership.CanIssueCertifications(),
            membership.HasRole(CommunityRole.Manager),
            membership.CertifiedAt ?? membership.DateJoined);

    public static IReadOnlyList<MemberSummaryDto> ToSummaryDtos(
        this IEnumerable<Membership> memberships, IReadOnlyDictionary<Guid, InternalProfileDto> profiles) =>
        memberships
            .Select(m => m.ToSummaryDto(profiles.GetValueOrDefault(m.AccountId)))
            .ToList();
}
