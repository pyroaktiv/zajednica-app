using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Community;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}/documents")]
public sealed class DocumentController : ControllerBase
{
    private readonly IDocumentService _documents;

    public DocumentController(IDocumentService documents)
    {
        _documents = documents;
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDto>> Add(Guid communityId, [FromBody] AddDocumentRequest request, CancellationToken ct)
    {
        return Ok(await _documents.AddAsync(User.AccountId(), communityId, request, ct));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<DocumentDto>>> GetPaged(Guid communityId, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
    {
        return Ok(await _documents.GetPagedAsync(User.AccountId(), communityId, page, pageSize, ct));
    }

    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> Remove(Guid communityId, Guid documentId, CancellationToken ct)
    {
        await _documents.RemoveAsync(User.AccountId(), communityId, documentId, ct);
        return NoContent();
    }
}
