namespace Zajednica.Community.Api.Dto;

public record CommunityMemberDto(
    Guid MembershipId,
    Guid AccountId,
    string Username,
    string? ImageUrl,
    bool IsConfirmed,
    int? Stars,
    IReadOnlyList<string> Roles);
