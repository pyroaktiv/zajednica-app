namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface IMembershipRepository
{
    void Add(Membership membership);
    void Update(Membership membership);

    Membership? GetById(Guid id);
    Membership? Get(Guid accountId, Guid communityId);
    IReadOnlyList<Membership> GetManyByIds(IReadOnlyCollection<Guid> ids);
    IReadOnlyList<Membership> GetByAccount(Guid accountId);
    IReadOnlyList<Membership> GetByCommunity(Guid communityId);
    IReadOnlyList<Membership> GetConfirmedByCommunity(Guid communityId);
    Membership? GetManager(Guid communityId);
    IReadOnlyList<Membership> GetWithExpiredMute(DateTime now);

    int CountConfirmed(Guid communityId);
}
