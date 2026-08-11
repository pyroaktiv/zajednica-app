using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Infrastructure.Database.Repositories;

internal sealed class EventSourcedIntentRepository(FeedDbContext db) : IIntentRepository
{
    public void Add(Intent intent)
    {
        db.IntentViews.Add(new IntentView(intent));
        db.IntentEvents.AddRange(intent.NewEvents);
    }

    public void Update(Intent intent)
    {
        if (intent.NewEvents.Count == 0)
            return;

        Reproject(intent);
        db.IntentEvents.AddRange(intent.NewEvents);
    }

    public Intent? Load(Guid id)
    {
        var stream = db.IntentEvents
            .AsNoTracking()
            .Where(e => e.StreamId == id)
            .OrderBy(e => e.Sequence)
            .ToList();

        return stream.Count == 0 ? null : Intent.Load(stream);
    }

    public IReadOnlyList<Intent> LoadOpenByTargetMembership(Guid communityId, Guid targetMembershipId) =>
        LoadStreams(db.IntentViews.AsNoTracking()
            .Where(v => v.CommunityId == communityId && v.TargetMembershipId == targetMembershipId && v.Status == IntentStatus.Open)
            .Select(v => v.Id));

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
}
