using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Comments;

namespace Zajednica.Feed.Api.Public;

public interface ICommentService
{
    CommentDto Add(Guid accountId, Guid communityId, Guid postId, AddCommentRequestDto requestDto);
    CommentDto Reply(Guid accountId, Guid communityId, Guid postId, Guid commentId, AddCommentRequestDto requestDto);

    CursorPage<CommentDto, PageCursor> GetRoots(Guid accountId, Guid communityId, Guid postId, PageCursor? after, int limit);
    CursorPage<CommentDto, PageCursor> GetReplies(Guid accountId, Guid communityId, Guid postId, Guid commentId, PageCursor? after, int limit);
}
