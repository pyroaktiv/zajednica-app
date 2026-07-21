namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface ICommunityRepository
{
    Task AddAsync(Community community, CancellationToken ct = default);
    Task UpdateAsync(Community community, CancellationToken ct = default);

    Task<Community?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Community?> GetByQrTokenAsync(string qrToken, CancellationToken ct = default);
    Task<IReadOnlyList<Community>> GetManyByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
}
