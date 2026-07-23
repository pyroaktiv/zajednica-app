using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface IDocumentRepository
{
    void Add(Document document);
    void Remove(Document document);

    Document? GetById(Guid id);
    PagedResult<Document> GetPaged(Guid communityId, int page, int pageSize);
}
