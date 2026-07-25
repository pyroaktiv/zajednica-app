namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface IBlacklistRepository
{
    void Add(BlacklistEntry entry);

    bool Exists(Guid accountId, Guid communityId);
}
