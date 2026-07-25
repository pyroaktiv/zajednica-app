using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Database.Repositories;

internal sealed class RefreshTokenEfRepository(IdentityDbContext db) : IRefreshTokenRepository
{
    public void Add(RefreshToken token)
    {
        db.RefreshTokens.Add(token);
        db.SaveChanges();
    }

    public void Remove(RefreshToken token)
    {
        db.RefreshTokens.Remove(token);
        db.SaveChanges();
    }

    public RefreshToken? GetByToken(string token) =>
        db.RefreshTokens.FirstOrDefault(t => t.Token == token);
}
