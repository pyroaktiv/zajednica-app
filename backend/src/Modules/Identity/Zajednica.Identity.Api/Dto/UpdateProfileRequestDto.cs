namespace Zajednica.Identity.Api.Dto;

public record UpdateProfileRequestDto(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail,
    string? ImageUrl);
