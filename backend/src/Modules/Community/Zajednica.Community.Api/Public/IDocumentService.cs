using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto.Documents;

namespace Zajednica.Community.Api.Public;

public interface IDocumentService
{
    DocumentDto Add(Guid accountId, Guid communityId, AddDocumentRequest request);
    PagedResult<DocumentDto> GetPaged(Guid accountId, Guid communityId, int page, int pageSize);
    void Remove(Guid accountId, Guid communityId, Guid documentId);
}
