using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Infrastructure.Database.Repositories;

internal sealed class PostEfRepository(FeedDbContext db) : IPostRepository
{
    public void Add(Post post)
    {
        db.Posts.Add(post);
        db.SaveChanges();
    }

    public void Update(Post post) => db.SaveChanges();

    public Post? Get(Guid id) =>
        db.Posts.FirstOrDefault(p => p.Id == id);

    public Post? GetWithComment(Guid postId, Guid commentId) =>
        db.Posts
            .Include(p => p.Comments.Where(c => c.Id == commentId))
            .FirstOrDefault(p => p.Id == postId);

    public bool Exists(Guid postId, Guid communityId) =>
        db.Posts.Any(p => p.Id == postId && p.CommunityId == communityId);

    public CursorPage<Post, DateTime> GetPage(Guid communityId, DateTime? before, int limit)
    {
        var query = db.Posts.AsNoTracking().Where(p => p.CommunityId == communityId);

        if (before is not null)
            query = query.Where(p => p.DateCreated < before);

        var items = query
            .OrderByDescending(p => p.DateCreated)
            .Take(limit)
            .ToList();

        return new CursorPage<Post, DateTime>(items, items.Count < limit ? null : items[^1].DateCreated);
    }

    public CursorPage<Comment, DateTime> GetCommentPage(Guid postId, Guid? parentCommentId, DateTime? after, int limit)
    {
        var query = parentCommentId is null
            ? db.Comments.AsNoTracking().Where(c => c.PostId == postId && c.ParentCommentId == null)
            : db.Comments.AsNoTracking().Where(c => c.PostId == postId && c.ParentCommentId == parentCommentId);

        if (after is not null)
            query = query.Where(c => c.Date > after);

        var items = query
            .OrderBy(c => c.Date)
            .Take(limit)
            .ToList();

        return new CursorPage<Comment, DateTime>(items, items.Count < limit ? null : items[^1].Date);
    }
}
