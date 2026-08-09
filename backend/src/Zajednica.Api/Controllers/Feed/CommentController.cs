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
    public ActionResult<CommentDto> Add(Guid communityId, Guid postId, [FromBody] AddCommentRequestDto requestDto)
    {
        return Ok(_comments.Add(User.AccountId(), communityId, postId, requestDto));
    }

    [HttpPost("{commentId:guid}/replies")]
    public ActionResult<CommentDto> Reply(Guid communityId, Guid postId, Guid commentId, [FromBody] AddCommentRequestDto requestDto)
    {
        return Ok(_comments.Reply(User.AccountId(), communityId, postId, commentId, requestDto));
    }

    [HttpGet]
    public ActionResult<CursorPage<CommentDto, PageCursor>> GetRoots(Guid communityId, Guid postId, [FromQuery] PageCursor? after, [FromQuery] int limit)
    {
        return Ok(_comments.GetRoots(User.AccountId(), communityId, postId, after, limit));
    }

    [HttpGet("{commentId:guid}/replies")]
    public ActionResult<CursorPage<CommentDto, PageCursor>> GetReplies(Guid communityId, Guid postId, Guid commentId, [FromQuery] PageCursor? after, [FromQuery] int limit)
    {
        return Ok(_comments.GetReplies(User.AccountId(), communityId, postId, commentId, after, limit));
    }
}
