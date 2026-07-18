namespace Zajednica.Identity.Api.Dto;

/// <summary>Logout revokes the presented refresh token, ending that session (rotation chain).</summary>
public record LogoutRequest(string RefreshToken);
