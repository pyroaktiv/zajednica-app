namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IEmailVerificationTokenRepository
{
    void Add(EmailVerificationToken token);
    void Remove(EmailVerificationToken token);

    EmailVerificationToken? GetByToken(string token);
}
