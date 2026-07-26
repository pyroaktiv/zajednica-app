using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Comments;

namespace Zajednica.Feed.Api.Public;

public interface ICommentService
{
    CommentDto Add(Guid accountId, Guid communityId, Guid postId, AddCommentRequest request);
    CommentDto Reply(Guid accountId, Guid communityId, Guid postId, Guid commentId, AddCommentRequest request);

    CursorPage<CommentDto> GetRoots(Guid accountId, Guid communityId, Guid postId, DateTime? after, int limit);
    CursorPage<CommentDto> GetReplies(Guid accountId, Guid communityId, Guid postId, Guid commentId, DateTime? after, int limit);
}
