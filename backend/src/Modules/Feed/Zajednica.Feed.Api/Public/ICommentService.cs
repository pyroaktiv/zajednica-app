using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Comments;

namespace Zajednica.Feed.Api.Public;

public interface ICommentService
{
    CommentDto Add(Guid accountId, Guid communityId, Guid postId, AddCommentRequest request);
    CommentDto Reply(Guid accountId, Guid communityId, Guid postId, Guid commentId, AddCommentRequest request);

    CursorPage<CommentDto, DateTime> GetRoots(Guid accountId, Guid communityId, Guid postId, DateTime? after, int limit);
    CursorPage<CommentDto, DateTime> GetReplies(Guid accountId, Guid communityId, Guid postId, Guid commentId, DateTime? after, int limit);
}
