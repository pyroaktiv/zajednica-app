using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Infrastructure.Database.Repositories;

internal sealed class IntentQueryStore(FeedDbContext db) : IIntentQueryStore
{
    public CursorPage<IntentView, PageCursor> GetPage(Guid communityId, PageCursor? before, int limit)
    {
        var query = db.IntentViews.AsNoTracking().Where(v => v.CommunityId == communityId);

        if (before is { } cursor)
            query = query.Where(v => v.DateCreated < cursor.At
                                     || (v.DateCreated == cursor.At && v.Id < cursor.Id));

        var items = query
            .OrderByDescending(v => v.DateCreated)
            .ThenByDescending(v => v.Id)
            .Take(limit + 1)
            .ToList();

        return Paging.ToPage(items, limit, v => new PageCursor(v.DateCreated, v.Id));
    }
    
    public IntentView? GetView(Guid intentId) =>
        db.IntentViews.AsNoTracking().FirstOrDefault(v => v.Id == intentId);

    public IReadOnlyList<IntentVoteView> GetVotes(Guid intentId) =>
        db.IntentVoteViews
            .AsNoTracking()
            .Where(v => v.IntentId == intentId)
            .OrderBy(v => v.OccurredAt)
            .ToList();

    public bool? GetVote(Guid intentId, Guid voterMembershipId) =>
        db.IntentVoteViews
            .AsNoTracking()
            .Where(v => v.IntentId == intentId && v.VoterMembershipId == voterMembershipId)
            .Select(v => (bool?)v.InFavor)
            .FirstOrDefault();
    
    public bool PostRatingIntentExists(Guid postId) =>
        db.IntentViews.AsNoTracking().Any(v => v.PostId == postId);

    public IReadOnlyList<Guid> GetDueIds(DateTime now) =>
        db.IntentViews.AsNoTracking()
            .Where(v => v.Deadline <= now && v.Status == IntentStatus.Open)
            .OrderBy(v => v.Deadline)
            .Select(v => v.Id)
            .ToList();
}