namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IEmailVerificationTokenRepository
{
    void Add(Verification token);
    void Remove(Verification token);

    Verification? GetByToken(string token);
}
