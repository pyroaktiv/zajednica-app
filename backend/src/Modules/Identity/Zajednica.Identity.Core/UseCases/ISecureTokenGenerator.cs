namespace Zajednica.Identity.Core.UseCases;

/// <summary>
/// Generates cryptographically-random, URL-safe opaque tokens. Used for both the refresh-token
/// value and the email-verification token. Implemented in Infrastructure (RNG adapter).
/// </summary>
public interface ISecureTokenGenerator
{
    string Generate();
}
