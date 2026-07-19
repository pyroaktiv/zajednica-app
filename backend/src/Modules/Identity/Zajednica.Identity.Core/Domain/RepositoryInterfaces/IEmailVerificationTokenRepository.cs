namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken ct = default);
    Task RemoveAsync(EmailVerificationToken token, CancellationToken ct = default);

    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default);
}
