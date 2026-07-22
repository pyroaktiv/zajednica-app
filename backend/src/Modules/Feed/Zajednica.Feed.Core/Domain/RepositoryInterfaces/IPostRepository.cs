using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Core.Domain.RepositoryInterfaces;

public interface IPostRepository
{
    Task AddAsync(Post post, CancellationToken ct = default);
    Task UpdateAsync(Post post, CancellationToken ct = default);

    Task<Post?> GetAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Post>> GetPagedAsync(Guid communityId, int page, int pageSize, CancellationToken ct = default);

    Task<Comment?> GetCommentAsync(Guid postId, Guid commentId, CancellationToken ct = default);
    Task<CursorPage<Comment>> GetCommentPageAsync(Guid postId, Guid? parentCommentId, Cursor? after, int limit,
        CancellationToken ct = default);
}
