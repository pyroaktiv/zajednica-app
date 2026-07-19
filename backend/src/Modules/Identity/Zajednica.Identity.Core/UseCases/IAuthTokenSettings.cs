namespace Zajednica.Identity.Core.UseCases;

/// <summary>
/// Token-lifecycle policy the application service reads (how long refresh / activation tokens stay
/// valid, where the activation link points). A Core-owned port, like the other Identity ports —
/// Infrastructure supplies the concrete values from configuration, so Core stays free of the
/// Options/Configuration packages.
/// </summary>
public interface IAuthTokenSettings
{
    /// <summary>How long a freshly issued (or rotated) refresh token stays valid.</summary>
    int RefreshTokenDays { get; }

    /// <summary>How long the account-activation token stays usable after registration.</summary>
    int EmailVerificationTokenHours { get; }

    /// <summary>Base URL the activation link points at; the token is appended as <c>?token=</c>.
    /// When blank, the email carries the bare token (dev fallback).</summary>
    string EmailActivationUrl { get; }
}
