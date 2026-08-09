namespace Zajednica.Community.Api.Dto.Memberships;

public record MemberSummaryDto(
    Guid MembershipId,
    Guid AccountId,
    string Username,
    string? FirstName,
    string? LastName,
    string? ImageUrl,
    bool IsConfirmed,
    int? Stars,
    IReadOnlyList<string> Roles);
