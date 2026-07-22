using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto.Documents;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;

namespace Zajednica.Community.Core.UseCases;

public sealed class DocumentService(
    IDocumentRepository documents,
    IRealtimePusher realtime,
    MembershipAccess access) : IDocumentService
{
    public async Task<DocumentDto> AddAsync(Guid accountId, Guid communityId, AddDocumentRequest request, CancellationToken ct = default)
    {
        var (_, actor) = await access.RequireRoleAsync(accountId, communityId, CommunityRole.Manager, ct);

        var document = new Document(communityId, actor.Id, request.Name, request.Url, DateTime.UtcNow);
        await documents.AddAsync(document, ct);

        await realtime.PushToChannelAsync(Channels.Community(communityId),
            new RealtimeMessage("community.documents.changed", new { communityId }), ct);

        return document.ToDto();
    }

    public async Task<PagedResult<DocumentDto>> GetPagedAsync(Guid accountId, Guid communityId, int page, int pageSize, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);

        var paged = await documents.GetPagedAsync(communityId, page, pageSize, ct);
        return new PagedResult<DocumentDto>(paged.Results.Select(d => d.ToDto()).ToList(), paged.TotalCount);
    }

    public async Task RemoveAsync(Guid accountId, Guid communityId, Guid documentId, CancellationToken ct = default)
    {
        await access.RequireRoleAsync(accountId, communityId, CommunityRole.Manager, ct);

        var document = await documents.GetByIdAsync(documentId, ct);
        if (document is null || document.CommunityId != communityId)
            throw new NotFoundException("Document not found in this community.");

        await documents.RemoveAsync(document, ct);

        await realtime.PushToChannelAsync(Channels.Community(communityId),
            new RealtimeMessage("community.documents.changed", new { communityId }), ct);
    }
}
