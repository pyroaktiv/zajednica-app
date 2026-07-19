namespace Zajednica.Identity.Api.Internal.Dto;

public record AccountProfileDto(
    Guid AccountId,
    string Username,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail,
    string? ImageUrl);
