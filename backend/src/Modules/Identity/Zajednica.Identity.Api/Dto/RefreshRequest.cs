namespace Zajednica.Identity.Api.Dto;

/// <summary>
/// Exchanges a valid refresh token for a fresh token pair. Rotation: the presented refresh token
/// is invalidated and a new one is issued alongside the new access token.
/// </summary>
public record RefreshRequest(string RefreshToken);
