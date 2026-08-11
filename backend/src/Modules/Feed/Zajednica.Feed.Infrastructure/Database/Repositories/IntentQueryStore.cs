using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;
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
        db.IntentEvents
            .AsNoTracking()
            .OfType<VoteCast>()
            .Where(e => e.StreamId == intentId)
            .OrderBy(e => e.Sequence)
            .Select(e => new IntentVoteView(e.VoterMembershipId, e.InFavor, e.OccurredAt))
            .ToList();

    public bool? GetVote(Guid intentId, Guid voterMembershipId) =>
        db.IntentEvents
            .AsNoTracking()
            .OfType<VoteCast>()
            .Where(e => e.StreamId == intentId && e.VoterMembershipId == voterMembershipId)
            .Select(e => (bool?)e.InFavor)
            .FirstOrDefault();
    
    public IReadOnlyList<Guid> GetDueIds(DateTime now) =>
        db.IntentViews.AsNoTracking()
            .Where(v => v.Deadline <= now && v.Status == IntentStatus.Open)
            .OrderBy(v => v.Deadline)
            .Select(v => v.Id)
            .ToList();
}