using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class BlacklistEfRepository(CommunityDbContext db) : IBlacklistRepository
{
    public void Add(BlacklistEntry entry)
    {
        db.BlacklistEntries.Add(entry);
        db.SaveChanges();
    }

    public bool Exists(Guid accountId, Guid communityId) =>
        db.BlacklistEntries.Any(b => b.AccountId == accountId && b.CommunityId == communityId);
}
