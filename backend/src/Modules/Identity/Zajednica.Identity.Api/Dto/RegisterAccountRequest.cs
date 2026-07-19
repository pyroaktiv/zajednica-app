namespace Zajednica.Identity.Api.Dto;

public record RegisterAccountRequest(
    string Username,
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail);
