namespace Zajednica.Identity.Api.Internal.Dto;

/// <summary>
/// All the data Identity owns about a user: the account handle plus the optional profile fields.
/// Deliberately excludes data owned by other modules (e.g. unit number, stars, roles live on
/// Community's Membership). Nullable fields are absent personal data.
/// </summary>
public record AccountProfileDto(
    Guid AccountId,
    string Username,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail,
    string? ImageUrl);
