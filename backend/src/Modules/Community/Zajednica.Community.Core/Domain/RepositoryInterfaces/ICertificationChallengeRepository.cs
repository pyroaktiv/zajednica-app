namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface ICertificationChallengeRepository
{
    void Add(CertificationChallenge challenge);
    void Remove(CertificationChallenge challenge);

    CertificationChallenge? GetById(Guid id);
    CertificationChallenge? GetByToken(string token);
}
