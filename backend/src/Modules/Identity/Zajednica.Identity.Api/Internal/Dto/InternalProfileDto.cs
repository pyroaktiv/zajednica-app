namespace Zajednica.Identity.Api.Internal.Dto;

public record InternalProfileDto(
    Guid AccountId,
    string Username,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? ContactEmail,
    string? ImageUrl);
