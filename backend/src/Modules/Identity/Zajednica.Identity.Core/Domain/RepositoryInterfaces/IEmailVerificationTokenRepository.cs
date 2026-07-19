namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IEmailVerificationTokenRepository
{
    void Add(EmailVerificationToken token);
    void Remove(EmailVerificationToken token);

    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default);
}
