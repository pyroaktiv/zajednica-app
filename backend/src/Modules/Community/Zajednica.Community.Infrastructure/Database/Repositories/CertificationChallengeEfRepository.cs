using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class CertificationChallengeEfRepository(CommunityDbContext db) : ICertificationChallengeRepository
{
    public Task AddAsync(CertificationChallenge challenge, CancellationToken ct = default)
    {
        db.CertificationChallenges.Add(challenge);
        return db.SaveChangesAsync(ct);
    }

    public Task RemoveAsync(CertificationChallenge challenge, CancellationToken ct = default)
    {
        db.CertificationChallenges.Remove(challenge);
        return db.SaveChangesAsync(ct);
    }

    public Task<CertificationChallenge?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.CertificationChallenges.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<CertificationChallenge?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        db.CertificationChallenges.FirstOrDefaultAsync(c => c.Token == token, ct);
}
