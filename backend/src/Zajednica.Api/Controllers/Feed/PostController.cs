using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Posts;
using Zajednica.Feed.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Feed;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}/posts")]
public sealed class PostController : ControllerBase
{
    private readonly IPostService _posts;
    private readonly IFileStorage _storage;

    public PostController(IPostService posts, IFileStorage storage)
    {
        _posts = posts;
        _storage = storage;
    }

    [HttpPost]
    public ActionResult<PostDto> CreateGeneral(Guid communityId, [FromBody] CreateGeneralPostRequestDto requestDto)
    {
        var created = _posts.CreateGeneral(User.AccountId(), communityId, requestDto);
        return CreatedAtAction(nameof(Get), new { communityId, postId = created.Id }, created);
    }

    [HttpPost("help-requests")]
    public ActionResult<PostDto> CreateHelpRequest(Guid communityId, [FromBody] CreateHelpRequestPostDto request)
    {
        var created = _posts.CreateHelpRequest(User.AccountId(), communityId, request);
        return CreatedAtAction(nameof(Get), new { communityId, postId = created.Id }, created);
    }

    [HttpPost("{postId:guid}/close")]
    public ActionResult<PostDto> CloseHelpRequest(Guid communityId, Guid postId)
    {
        return Ok(_posts.CloseHelpRequest(User.AccountId(), communityId, postId));
    }

    [HttpGet("{postId:guid}")]
    public ActionResult<PostDto> Get(Guid communityId, Guid postId)
    {
        return Ok(_posts.Get(User.AccountId(), communityId, postId));
    }

    [HttpGet]
    public ActionResult<CursorPage<PostDto, PageCursor>> GetPage(Guid communityId, [FromQuery] PageCursor? before, [FromQuery] int limit)
    {
        return Ok(_posts.GetPage(User.AccountId(), communityId, before, limit));
    }

    [HttpGet("{postId:guid}/images/{index:int}/content")]
    public IActionResult ImageContent(Guid communityId, Guid postId, int index)
    {
        var reference = _posts.GetImageContent(User.AccountId(), communityId, postId, index);
        var file = _storage.Open(reference.Key);
        if (file is null)
            return NotFound();

        Response.Headers.CacheControl = "private, max-age=300";
        return File(file.Content, file.ContentType, reference.DownloadName, enableRangeProcessing: true);
    }
}
