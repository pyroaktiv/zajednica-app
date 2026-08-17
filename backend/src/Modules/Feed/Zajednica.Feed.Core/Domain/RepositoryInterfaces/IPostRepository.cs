using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Core.Domain.RepositoryInterfaces;

public interface IPostRepository
{
    void Add(Post post);

    Post? Get(Guid id);
    Post? GetWithComment(Guid postId, Guid commentId);
    bool Exists(Guid postId, Guid communityId);
    CursorPage<Post, PageCursor> GetPage(Guid communityId, PageCursor? before, int limit);

    CursorPage<Comment, PageCursor> GetCommentPage(Guid postId, Guid? parentCommentId, PageCursor? after, int limit);
}
