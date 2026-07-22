using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Comments;
using Zajednica.Feed.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Feed;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}/posts/{postId:guid}/comments")]
public sealed class CommentController : ControllerBase
{
    private readonly ICommentService _comments;

    public CommentController(ICommentService comments)
    {
        _comments = comments;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> Add(Guid communityId, Guid postId, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        return Ok(await _comments.AddAsync(User.AccountId(), communityId, postId, request, ct));
    }

    [HttpPost("{commentId:guid}/replies")]
    public async Task<ActionResult<CommentDto>> Reply(Guid communityId, Guid postId, Guid commentId, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        return Ok(await _comments.ReplyAsync(User.AccountId(), communityId, postId, commentId, request, ct));
    }

    [HttpGet]
    public async Task<ActionResult<CursorPage<CommentDto>>> GetRoots(Guid communityId, Guid postId, [FromQuery] string? cursor, [FromQuery] int limit, CancellationToken ct)
    {
        return Ok(await _comments.GetRootsAsync(User.AccountId(), communityId, postId, cursor, limit, ct));
    }

    [HttpGet("{commentId:guid}/replies")]
    public async Task<ActionResult<CursorPage<CommentDto>>> GetReplies(Guid communityId, Guid postId, Guid commentId, [FromQuery] string? cursor, [FromQuery] int limit, CancellationToken ct)
    {
        return Ok(await _comments.GetRepliesAsync(User.AccountId(), communityId, postId, commentId, cursor, limit, ct));
    }
}
