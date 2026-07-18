namespace Zajednica.Identity.Api.Dto;

/// <summary>
/// Registration input: account credentials plus optional profile data (spec §1, §2).
/// Everything but the image is collected here; ImageUrl is set later through profile update.
/// </summary>
public record RegisterAccountRequest(
    string Username,
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail);
