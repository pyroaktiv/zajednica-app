using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Core.Domain.RepositoryInterfaces;

public interface IPostRepository
{
    void Add(Post post);
    void Update(Post post);

    Post? Get(Guid id);
    CursorPage<Post> GetPage(Guid communityId, DateTime? before, int limit);

    Comment? GetComment(Guid postId, Guid commentId);
    CursorPage<Comment> GetCommentPage(Guid postId, Guid? parentCommentId, DateTime? after, int limit);
}
