using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class CommunityEfRepository(CommunityDbContext db) : ICommunityRepository
{
    public void Add(Core.Domain.Community community)
    {
        db.Communities.Add(community);
        db.SaveChanges();
    }

    public void Update(Core.Domain.Community community) => db.SaveChanges();

    public Core.Domain.Community? GetById(Guid id) =>
        db.Communities.FirstOrDefault(c => c.Id == id);

    public Core.Domain.Community? GetByQrToken(string qrToken) =>
        db.Communities.FirstOrDefault(c => c.QrToken == qrToken);

    public IReadOnlyList<Core.Domain.Community> GetManyByIds(IReadOnlyCollection<Guid> ids) =>
        db.Communities.Where(c => ids.Contains(c.Id)).ToList();
}
