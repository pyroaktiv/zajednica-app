namespace Zajednica.Community.Api.Dto.Memberships;

public record MyMembershipDto(
    Guid MembershipId,
    Guid CommunityId,
    string? UnitNumber,
    bool IsConfirmed,
    int? Stars,
    IReadOnlyList<string> Roles,
    DateTime DateJoined);
