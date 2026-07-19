namespace Zajednica.Identity.Api.Dto;

public record LoginRequest(string UsernameOrEmail, string Password);
