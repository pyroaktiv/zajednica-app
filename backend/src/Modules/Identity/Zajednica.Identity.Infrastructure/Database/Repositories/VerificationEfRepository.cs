using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Database.Repositories;

internal sealed class VerificationEfRepository(IdentityDbContext db) : IVerificationRepository
{
    public void Add(Verification token)
    {
        db.Verifications.Add(token);
        db.SaveChanges();
    }

    public void Remove(Verification token)
    {
        db.Verifications.Remove(token);
        db.SaveChanges();
    }

    public Verification? GetByToken(string token) =>
        db.Verifications.FirstOrDefault(t => t.Token == token);
}
