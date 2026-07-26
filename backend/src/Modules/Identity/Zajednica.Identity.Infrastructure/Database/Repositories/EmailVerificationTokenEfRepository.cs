using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Database.Repositories;

internal sealed class EmailVerificationTokenEfRepository(IdentityDbContext db) : IEmailVerificationTokenRepository
{
    public void Add(Verification token)
    {
        db.EmailVerificationTokens.Add(token);
        db.SaveChanges();
    }

    public void Remove(Verification token)
    {
        db.EmailVerificationTokens.Remove(token);
        db.SaveChanges();
    }

    public Verification? GetByToken(string token) =>
        db.EmailVerificationTokens.FirstOrDefault(t => t.Token == token);
}
