using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public PostController(IPostService posts)
    {
        _posts = posts;
    }

    [HttpPost]
    public ActionResult<PostDto> CreateGeneral(Guid communityId, [FromBody] CreateGeneralPostRequest request)
    {
        var created = _posts.CreateGeneral(User.AccountId(), communityId, request);
        return CreatedAtAction(nameof(Get), new { communityId, postId = created.Id }, created);
    }

    [HttpPost("help-requests")]
    public ActionResult<PostDto> CreateHelpRequest(Guid communityId, [FromBody] CreateHelpRequestRequest request)
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
}
