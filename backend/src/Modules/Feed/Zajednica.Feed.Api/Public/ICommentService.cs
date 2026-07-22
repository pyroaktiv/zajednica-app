using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Comments;

namespace Zajednica.Feed.Api.Public;

public interface ICommentService
{
    Task<CommentDto> AddAsync(Guid accountId, Guid communityId, Guid postId, AddCommentRequest request, CancellationToken ct = default);
    Task<CommentDto> ReplyAsync(Guid accountId, Guid communityId, Guid postId, Guid commentId, AddCommentRequest request, CancellationToken ct = default);

    Task<CursorPage<CommentDto>> GetRootsAsync(Guid accountId, Guid communityId, Guid postId, string? cursor, int limit, CancellationToken ct = default);
    Task<CursorPage<CommentDto>> GetRepliesAsync(Guid accountId, Guid communityId, Guid postId, Guid commentId, string? cursor, int limit, CancellationToken ct = default);
}
