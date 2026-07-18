namespace Zajednica.Identity.Api.Internal.Dto;

/// <summary>Minimal projection: an account id paired with its username, for batch resolution.</summary>
public record AccountUsernameDto(Guid AccountId, string Username);
