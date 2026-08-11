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
    IDocumentRepository documentRepository,
    IFileUrlMapper urlMapper,
    MembershipRequirementsService requirementsService) : IDocumentService
{
    public DocumentDto Add(Guid accountId, Guid communityId, AddDocumentRequestDto requestDto)
    {
        var (_, actor) = requirementsService.RequireRole(accountId, communityId, CommunityRole.Manager);

        var document = new Document(communityId, actor.Id, requestDto.Name, urlMapper.ToKey(requestDto.Url)!, DateTime.UtcNow);
        documentRepository.Add(document);

        return document.ToDto(urlMapper);
    }

    public PagedResult<DocumentDto> GetPaged(Guid accountId, Guid communityId, int page, int pageSize)
    {
        requirementsService.RequireConfirmed(accountId, communityId);

        var paged = documentRepository.GetPaged(communityId, page, pageSize);
        return new PagedResult<DocumentDto>(paged.Results.Select(d => d.ToDto(urlMapper)).ToList(), paged.TotalCount);
    }

    public void Remove(Guid accountId, Guid communityId, Guid documentId)
    {
        requirementsService.RequireRole(accountId, communityId, CommunityRole.Manager);

        var document = documentRepository.GetById(documentId);
        if (document is null || document.CommunityId != communityId)
            throw new NotFoundException("Document not found in this community.");

        documentRepository.Remove(document);
    }
}
