using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Core.Domain.RepositoryInterfaces;

public interface IPostRepository
{
    void Add(Post post);
    void Update(Post post);

    Post? Get(Guid id);
    Post? GetWithComment(Guid postId, Guid commentId);
    bool Exists(Guid postId, Guid communityId);
    CursorPage<Post, DateTime> GetPage(Guid communityId, DateTime? before, int limit);

    CursorPage<Comment, DateTime> GetCommentPage(Guid postId, Guid? parentCommentId, DateTime? after, int limit);
}
