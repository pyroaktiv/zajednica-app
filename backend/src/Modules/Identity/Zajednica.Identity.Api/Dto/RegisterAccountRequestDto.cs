namespace Zajednica.Identity.Api.Dto;

public record RegisterAccountRequestDto(
    string Username,
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail);
