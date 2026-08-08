using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Infrastructure.Database.Repositories;

internal sealed class EventSourcedIntentRepository(FeedDbContext db) : IIntentRepository, IIntentQueryStore
{
    public void Add(Intent intent)
    {
        db.IntentViews.Add(new IntentView(intent));
        Append(intent);
    }

    public void Update(Intent intent)
    {
        if (intent.NewEvents.Count == 0)
            return;

        Reproject(intent);
        Append(intent);
    }

    public Intent? LoadFromSource(Guid id)
    {
        var stream = db.IntentEvents
            .AsNoTracking()
            .Where(e => e.StreamId == id)
            .OrderBy(e => e.Sequence)
            .ToList();

        return stream.Count == 0 ? null : Intent.Load(stream);
    }

    public CursorPage<IntentView, PageCursor> GetPage(Guid communityId, PageCursor? before, int limit)
    {
        var query = db.IntentViews.AsNoTracking().Where(v => v.CommunityId == communityId);

        if (before is { } cursor)
            query = query.Where(v => v.DateCreated < cursor.At
                                     || (v.DateCreated == cursor.At && v.Id.CompareTo(cursor.Id) < 0));

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
        OpenViews()
            .Where(v => v.Deadline <= now)
            .OrderBy(v => v.Deadline)
            .Select(v => v.Id)
            .ToList();

    public IReadOnlyList<Intent> GetOpenByTarget(Guid communityId, Guid targetMembershipId) =>
        LoadStreams(OpenViews()
            .Where(v => v.CommunityId == communityId && v.TargetMembershipId == targetMembershipId)
            .Select(v => v.Id));

    private IQueryable<IntentView> OpenViews() =>
        db.IntentViews.AsNoTracking().Where(v => v.Status == IntentStatus.Open);

    private IReadOnlyList<Intent> LoadStreams(IQueryable<Guid> streamIds) =>
        db.IntentEvents
            .AsNoTracking()
            .Where(e => streamIds.Contains(e.StreamId))
            .OrderBy(e => e.StreamId).ThenBy(e => e.Sequence)
            .ToList()
            .GroupBy(e => e.StreamId)
            .Select(stream => Intent.Load(stream.ToList()))
            .ToList();

    private void Reproject(Intent intent)
    {
        if (db.IntentViews.Local.FirstOrDefault(v => v.Id == intent.Id) is { } tracked)
            tracked.Update(intent);
        else
            db.IntentViews.Update(new IntentView(intent));
    }

    private void Append(Intent intent)
    {
        db.IntentEvents.AddRange(intent.NewEvents);
        db.SaveChanges();
        intent.ClearNewEvents();
    }
}
