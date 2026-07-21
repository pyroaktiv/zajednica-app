using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto;

namespace Zajednica.Community.Api.Public;

public interface IDocumentService
{
    Task<DocumentDto> AddAsync(Guid accountId, Guid communityId, AddDocumentRequest request, CancellationToken ct = default);
    Task<PagedResult<DocumentDto>> GetPagedAsync(Guid accountId, Guid communityId, int page, int pageSize, CancellationToken ct = default);
    Task RemoveAsync(Guid accountId, Guid communityId, Guid documentId, CancellationToken ct = default);
}
