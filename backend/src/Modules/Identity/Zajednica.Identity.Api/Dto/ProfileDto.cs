namespace Zajednica.Identity.Api.Dto;

public record ProfileDto(
    string Username,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail,
    string? ImageUrl);
