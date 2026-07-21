using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken ct = default);
    Task RemoveAsync(Document document, CancellationToken ct = default);

    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Document>> GetPagedAsync(Guid communityId, int page, int pageSize, CancellationToken ct = default);
}
