namespace Zajednica.Identity.Api.Dto;

/// <summary>
/// The token pair returned by login / register / refresh. The access token is a short-lived JWT
/// (validated statelessly); the refresh token is long-lived and rotated on every refresh.
/// </summary>
public record AuthTokens(string AccessToken, string RefreshToken);
