namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface IBlacklistRepository
{
    Task AddAsync(BlacklistEntry entry, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
}
