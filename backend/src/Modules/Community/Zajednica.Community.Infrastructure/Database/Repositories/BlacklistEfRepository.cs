using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class BlacklistEfRepository(CommunityDbContext db) : IBlacklistRepository
{
    public Task AddAsync(BlacklistEntry entry, CancellationToken ct = default)
    {
        db.BlacklistEntries.Add(entry);
        return db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid accountId, Guid communityId, CancellationToken ct = default) =>
        db.BlacklistEntries.AnyAsync(b => b.AccountId == accountId && b.CommunityId == communityId, ct);
}
