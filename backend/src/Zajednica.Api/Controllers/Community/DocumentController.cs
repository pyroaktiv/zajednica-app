using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto.Documents;
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
    public ActionResult<DocumentDto> Add(Guid communityId, [FromBody] AddDocumentRequest request)
    {
        return Ok(_documents.Add(User.AccountId(), communityId, request));
    }

    [HttpGet]
    public ActionResult<PagedResult<DocumentDto>> GetPaged(Guid communityId, [FromQuery] int page, [FromQuery] int pageSize)
    {
        return Ok(_documents.GetPaged(User.AccountId(), communityId, page, pageSize));
    }

    [HttpDelete("{documentId:guid}")]
    public IActionResult Remove(Guid communityId, Guid documentId)
    {
        _documents.Remove(User.AccountId(), communityId, documentId);
        return NoContent();
    }
}
