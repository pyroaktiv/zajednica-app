namespace Zajednica.Identity.Api.Dto;

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail,
    string? ImageUrl);
