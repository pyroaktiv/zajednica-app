using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class CommunityEfRepository(CommunityDbContext db) : ICommunityRepository
{
    public Task AddAsync(Core.Domain.Community community, CancellationToken ct = default)
    {
        db.Communities.Add(community);
        return db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Core.Domain.Community community, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<Core.Domain.Community?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Communities.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Core.Domain.Community?> GetByQrTokenAsync(string qrToken, CancellationToken ct = default) =>
        db.Communities.FirstOrDefaultAsync(c => c.QrToken == qrToken, ct);

    public async Task<IReadOnlyList<Core.Domain.Community>> GetManyByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.Communities.Where(c => ids.Contains(c.Id)).ToListAsync(ct);
}
