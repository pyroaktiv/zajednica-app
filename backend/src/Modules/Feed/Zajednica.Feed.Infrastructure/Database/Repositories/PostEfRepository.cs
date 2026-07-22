using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.BuildingBlocks.Infrastructure.Database;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Infrastructure.Database.Repositories;

internal sealed class PostEfRepository(FeedDbContext db) : IPostRepository
{
    public Task AddAsync(Post post, CancellationToken ct = default)
    {
        db.Posts.Add(post);
        return db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Post post, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<Post?> GetAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<PagedResult<Post>> GetPagedAsync(Guid communityId, int page, int pageSize, CancellationToken ct = default) =>
        db.Posts
            .Where(p => p.CommunityId == communityId)
            .OrderByDescending(p => db.Set<GeneralTopicPost>()
                .Any(g => g.Id == p.Id && g.Kind == GeneralPostKind.Emergency))
            .ThenByDescending(p => p.DateCreated)
            .GetPaged(page, pageSize);

    public Task<Comment?> GetCommentAsync(Guid postId, Guid commentId, CancellationToken ct = default) =>
        db.Comments.FirstOrDefaultAsync(c => c.PostId == postId && c.Id == commentId, ct);

    public async Task<CursorPage<Comment>> GetCommentPageAsync(Guid postId, Guid? parentCommentId, Cursor? after, int limit,
        CancellationToken ct = default)
    {
        var query = parentCommentId is null
            ? db.Comments.AsNoTracking().Where(c => c.PostId == postId && c.ParentCommentId == null)
            : db.Comments.AsNoTracking().Where(c => c.PostId == postId && c.ParentCommentId == parentCommentId);

        if (after is not null)
            query = query.Where(c => c.Date > after.Date || (c.Date == after.Date && c.Id > after.Id));

        var rows = await query
            .OrderBy(c => c.Date)
            .ThenBy(c => c.Id)
            .Take(limit + 1)
            .ToListAsync(ct);

        if (rows.Count <= limit)
            return new CursorPage<Comment>(rows, null);

        var items = rows.Take(limit).ToList();
        return new CursorPage<Comment>(items, new Cursor(items[^1].Date, items[^1].Id).Encode());
    }
}
