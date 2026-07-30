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

    public CursorPage<Post, PageCursor> GetPage(Guid communityId, PageCursor? before, int limit)
    {
        var query = db.Posts.AsNoTracking().Where(p => p.CommunityId == communityId);

        if (before is { } cursor)
            query = query.Where(p => p.DateCreated < cursor.At
                                     || (p.DateCreated == cursor.At && p.Id.CompareTo(cursor.Id) < 0));

        var items = query
            .OrderByDescending(p => p.DateCreated)
            .ThenByDescending(p => p.Id)
            .Take(limit)
            .ToList();

        return new CursorPage<Post, PageCursor>(items,
            items.Count < limit ? null : new PageCursor(items[^1].DateCreated, items[^1].Id));
    }

    public CursorPage<Comment, PageCursor> GetCommentPage(Guid postId, Guid? parentCommentId, PageCursor? after, int limit)
    {
        var query = parentCommentId is null
            ? db.Comments.AsNoTracking().Where(c => c.PostId == postId && c.ParentCommentId == null)
            : db.Comments.AsNoTracking().Where(c => c.PostId == postId && c.ParentCommentId == parentCommentId);

        if (after is { } cursor)
            query = query.Where(c => c.Date > cursor.At
                                     || (c.Date == cursor.At && c.Id.CompareTo(cursor.Id) > 0));

        var items = query
            .OrderBy(c => c.Date)
            .ThenBy(c => c.Id)
            .Take(limit)
            .ToList();

        return new CursorPage<Comment, PageCursor>(items,
            items.Count < limit ? null : new PageCursor(items[^1].Date, items[^1].Id));
    }
}
