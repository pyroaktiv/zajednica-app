namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IVerificationRepository
{
    void Add(Verification token);
    void Remove(Verification token);

    Verification? GetByToken(string token);
}
