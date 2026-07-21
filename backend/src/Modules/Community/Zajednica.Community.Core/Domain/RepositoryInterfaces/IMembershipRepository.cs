namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface IMembershipRepository
{
    Task AddAsync(Membership membership, CancellationToken ct = default);
    Task UpdateAsync(Membership membership, CancellationToken ct = default);

    Task<Membership?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Membership?> GetAsync(Guid accountId, Guid communityId, CancellationToken ct = default);
    Task<IReadOnlyList<Membership>> GetManyByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Membership>> GetByAccountAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<Membership>> GetByCommunityAsync(Guid communityId, CancellationToken ct = default);

    Task<int> CountConfirmedAsync(Guid communityId, CancellationToken ct = default);
}
