namespace Zajednica.Community.Api.Dto.Memberships;

public record MemberProfileDto(
    Guid MembershipId,
    Guid AccountId,
    string Username,
    string? ImageUrl,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail,
    string? UnitNumber,
    bool IsConfirmed,
    int? Stars,
    IReadOnlyList<string> Roles,
    DateTime DateJoined,
    string State);
