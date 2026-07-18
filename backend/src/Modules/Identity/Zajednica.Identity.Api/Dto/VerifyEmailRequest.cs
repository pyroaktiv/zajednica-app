namespace Zajednica.Identity.Api.Dto;

/// <summary>Carries the activation token from the emailed link (spec §1).</summary>
public record VerifyEmailRequest(string Token);
