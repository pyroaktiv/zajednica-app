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
    public async Task<ActionResult<PostDto>> CreateGeneral(Guid communityId, [FromBody] CreateGeneralPostRequest request, CancellationToken ct)
    {
        var created = await _posts.CreateGeneralAsync(User.AccountId(), communityId, request, ct);
        return CreatedAtAction(nameof(Get), new { communityId, postId = created.Id }, created);
    }

    [HttpPost("help-requests")]
    public async Task<ActionResult<PostDto>> CreateHelpRequest(Guid communityId, [FromBody] CreateHelpRequestRequest request, CancellationToken ct)
    {
        var created = await _posts.CreateHelpRequestAsync(User.AccountId(), communityId, request, ct);
        return CreatedAtAction(nameof(Get), new { communityId, postId = created.Id }, created);
    }

    [HttpPost("{postId:guid}/close")]
    public async Task<ActionResult<PostDto>> CloseHelpRequest(Guid communityId, Guid postId, CancellationToken ct)
    {
        return Ok(await _posts.CloseHelpRequestAsync(User.AccountId(), communityId, postId, ct));
    }

    [HttpGet("{postId:guid}")]
    public async Task<ActionResult<PostDto>> Get(Guid communityId, Guid postId, CancellationToken ct)
    {
        return Ok(await _posts.GetAsync(User.AccountId(), communityId, postId, ct));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PostDto>>> GetPaged(Guid communityId, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
    {
        return Ok(await _posts.GetPagedAsync(User.AccountId(), communityId, page, pageSize, ct));
    }
}
