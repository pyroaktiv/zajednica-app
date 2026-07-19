using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Database.Repositories;

internal sealed class RefreshTokenEfRepository(IdentityDbContext db) : IRefreshTokenRepository
{
    public Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        db.RefreshTokens.Add(token);
        return db.SaveChangesAsync(ct);
    }

    public Task RemoveAsync(RefreshToken token, CancellationToken ct = default)
    {
        db.RefreshTokens.Remove(token);
        return db.SaveChangesAsync(ct);
    }

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
}
