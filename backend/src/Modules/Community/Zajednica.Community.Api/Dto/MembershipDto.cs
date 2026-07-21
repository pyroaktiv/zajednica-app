namespace Zajednica.Community.Api.Dto;

public record MembershipDto(
    CommunityMemberDto Member,
    string? UnitNumber,
    DateTime? MutedUntil,
    DateTime DateJoined,
    string State);
