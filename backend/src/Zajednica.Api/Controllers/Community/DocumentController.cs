using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.Storage;
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
    private readonly IFileStorage _storage;

    public DocumentController(IDocumentService documents, IFileStorage storage)
    {
        _documents = documents;
        _storage = storage;
    }

    [HttpPost]
    public ActionResult<DocumentDto> Add(Guid communityId, [FromBody] AddDocumentRequestDto requestDto)
    {
        return Ok(_documents.Add(User.AccountId(), communityId, requestDto));
    }

    [HttpGet]
    public ActionResult<PagedResult<DocumentDto>> GetPaged(Guid communityId, [FromQuery] int page, [FromQuery] int pageSize)
    {
        return Ok(_documents.GetPaged(User.AccountId(), communityId, page, pageSize));
    }

    [HttpGet("{documentId:guid}/content")]
    public IActionResult Content(Guid communityId, Guid documentId)
    {
        var reference = _documents.GetContent(User.AccountId(), communityId, documentId);
        var file = _storage.Open(reference.Key);
        if (file is null)
            return NotFound();

        Response.Headers.CacheControl = "private, max-age=300";
        return File(file.Content, file.ContentType, reference.DownloadName, enableRangeProcessing: true);
    }

    [HttpDelete("{documentId:guid}")]
    public IActionResult Remove(Guid communityId, Guid documentId)
    {
        _documents.Remove(User.AccountId(), communityId, documentId);
        return NoContent();
    }
}
