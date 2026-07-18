namespace Zajednica.Identity.Api.Dto;

/// <summary>Login accepts either the username or the email in a single field (spec §1).</summary>
public record LoginRequest(string UsernameOrEmail, string Password);
