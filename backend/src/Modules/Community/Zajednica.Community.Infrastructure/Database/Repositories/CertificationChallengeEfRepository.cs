using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class CertificationChallengeEfRepository(CommunityDbContext db) : ICertificationChallengeRepository
{
    public void Add(CertificationChallenge challenge)
    {
        db.CertificationChallenges.Add(challenge);
        db.SaveChanges();
    }

    public void Remove(CertificationChallenge challenge)
    {
        db.CertificationChallenges.Remove(challenge);
        db.SaveChanges();
    }

    public CertificationChallenge? GetById(Guid id) =>
        db.CertificationChallenges.FirstOrDefault(c => c.Id == id);

    public CertificationChallenge? GetByToken(string token) =>
        db.CertificationChallenges.FirstOrDefault(c => c.Token == token);
}
