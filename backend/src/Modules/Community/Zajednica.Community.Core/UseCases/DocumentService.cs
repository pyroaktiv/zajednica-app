using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto.Documents;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;

namespace Zajednica.Community.Core.UseCases;

public sealed class DocumentService(
    IDocumentRepository documents,
    IFileUrlMapper urls,
    MembershipAccess access) : IDocumentService
{
    public DocumentDto Add(Guid accountId, Guid communityId, AddDocumentRequest request)
    {
        var (_, actor) = access.RequireRole(accountId, communityId, CommunityRole.Manager);

        var document = new Document(communityId, actor.Id, request.Name, urls.ToKey(request.Url)!, DateTime.UtcNow);
        documents.Add(document);

        return document.ToDto(urls);
    }

    public PagedResult<DocumentDto> GetPaged(Guid accountId, Guid communityId, int page, int pageSize)
    {
        access.RequireConfirmed(accountId, communityId);

        var paged = documents.GetPaged(communityId, page, pageSize);
        return new PagedResult<DocumentDto>(paged.Results.Select(d => d.ToDto(urls)).ToList(), paged.TotalCount);
    }

    public void Remove(Guid accountId, Guid communityId, Guid documentId)
    {
        access.RequireRole(accountId, communityId, CommunityRole.Manager);

        var document = documents.GetById(documentId);
        if (document is null || document.CommunityId != communityId)
            throw new NotFoundException("Document not found in this community.");

        documents.Remove(document);
    }
}
