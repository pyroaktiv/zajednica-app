using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class MembershipEfRepository(CommunityDbContext db) : IMembershipRepository
{
    public Task AddAsync(Membership membership, CancellationToken ct = default)
    {
        db.Memberships.Add(membership);
        return db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Membership membership, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<Membership?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Memberships.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<Membership?> GetAsync(Guid accountId, Guid communityId, CancellationToken ct = default) =>
        db.Memberships.FirstOrDefaultAsync(m => m.AccountId == accountId && m.CommunityId == communityId, ct);

    public async Task<IReadOnlyList<Membership>> GetManyByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.Memberships.Where(m => ids.Contains(m.Id)).ToListAsync(ct);

    public async Task<IReadOnlyList<Membership>> GetByAccountAsync(Guid accountId, CancellationToken ct = default) =>
        await db.Memberships.Where(m => m.AccountId == accountId).ToListAsync(ct);

    public async Task<IReadOnlyList<Membership>> GetByCommunityAsync(Guid communityId, CancellationToken ct = default) =>
        await db.Memberships.Where(m => m.CommunityId == communityId).ToListAsync(ct);

    public Task<int> CountConfirmedAsync(Guid communityId, CancellationToken ct = default) =>
        db.Memberships.CountAsync(m =>
            m.CommunityId == communityId
            && m.CertificationStatus == CertificationStatus.Confirmed
            && m.State == MembershipState.Active, ct);
}
