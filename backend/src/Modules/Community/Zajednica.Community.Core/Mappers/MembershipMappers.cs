using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Community.Core.Domain;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Community.Core.Mappers;

public static class MembershipMappers
{
    public static CommunityMemberDto ToMemberDto(this Membership membership, AccountProfileDto? profile) =>
        new(membership.Id,
            membership.AccountId,
            profile?.Username ?? string.Empty,
            profile?.ImageUrl,
            membership.IsConfirmed(),
            membership.IsConfirmed() ? membership.Stars : null,
            membership.Roles.Select(r => r.Role.ToString()).ToList());

    public static MembershipDto ToDto(this Membership membership, AccountProfileDto? profile) =>
        new(membership.ToMemberDto(profile),
            membership.UnitNumber,
            membership.MutedUntil,
            membership.DateJoined,
            membership.State.ToString());

    public static MembershipContextDto ToContextDto(this Membership membership, DateTime now) =>
        new(membership.Id,
            membership.AccountId,
            membership.CommunityId,
            membership.IsConfirmed(),
            membership.IsActive(),
            membership.IsMuted(now),
            membership.Roles.Select(r => r.Role.ToString()).ToList());

    public static IReadOnlyList<CommunityMemberDto> ToMemberDtos(
        this IEnumerable<Membership> memberships, IReadOnlyDictionary<Guid, AccountProfileDto> profiles) =>
        memberships
            .Select(m => m.ToMemberDto(profiles.GetValueOrDefault(m.AccountId)))
            .ToList();
}
