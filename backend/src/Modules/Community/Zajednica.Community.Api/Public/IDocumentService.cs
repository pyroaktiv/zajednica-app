using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto.Documents;

namespace Zajednica.Community.Api.Public;

public interface IDocumentService
{
    DocumentDto Add(Guid accountId, Guid communityId, AddDocumentRequestDto requestDto);
    PagedResult<DocumentDto> GetPaged(Guid accountId, Guid communityId, int page, int pageSize);
    FileReference GetContent(Guid accountId, Guid communityId, Guid documentId);
    void Remove(Guid accountId, Guid communityId, Guid documentId);
}
