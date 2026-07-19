namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

/// <summary>Repository for the ephemeral email-verification token. Looked up by its value, then removed.</summary>
public interface IEmailVerificationTokenRepository
{
    void Add(EmailVerificationToken token);
    void Remove(EmailVerificationToken token);

    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default);
}
