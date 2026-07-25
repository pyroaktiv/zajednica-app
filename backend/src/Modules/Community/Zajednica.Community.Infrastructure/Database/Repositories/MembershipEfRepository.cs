using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class MembershipEfRepository(CommunityDbContext db) : IMembershipRepository
{
    public void Add(Membership membership)
    {
        db.Memberships.Add(membership);
        db.SaveChanges();
    }

    public void Update(Membership membership) => db.SaveChanges();

    public Membership? GetById(Guid id) =>
        db.Memberships.FirstOrDefault(m => m.Id == id);

    public Membership? Get(Guid accountId, Guid communityId) =>
        db.Memberships.FirstOrDefault(m => m.AccountId == accountId && m.CommunityId == communityId);

    public IReadOnlyList<Membership> GetManyByIds(IReadOnlyCollection<Guid> ids) =>
        db.Memberships.Where(m => ids.Contains(m.Id)).ToList();

    public IReadOnlyList<Membership> GetByAccount(Guid accountId) =>
        db.Memberships.Where(m => m.AccountId == accountId).ToList();

    public IReadOnlyList<Membership> GetByCommunity(Guid communityId) =>
        db.Memberships.Where(m => m.CommunityId == communityId).ToList();

    public int CountConfirmed(Guid communityId) =>
        db.Memberships.Count(m =>
            m.CommunityId == communityId
            && m.CertificationStatus == CertificationStatus.Confirmed
            && m.State == MembershipState.Active);
}
