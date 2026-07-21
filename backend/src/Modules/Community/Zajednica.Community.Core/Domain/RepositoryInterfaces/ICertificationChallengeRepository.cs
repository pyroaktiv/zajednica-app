namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface ICertificationChallengeRepository
{
    Task AddAsync(CertificationChallenge challenge, CancellationToken ct = default);
    Task RemoveAsync(CertificationChallenge challenge, CancellationToken ct = default);

    Task<CertificationChallenge?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CertificationChallenge?> GetByTokenAsync(string token, CancellationToken ct = default);
}
