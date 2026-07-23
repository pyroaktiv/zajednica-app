using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Infrastructure.Database.Repositories;

internal sealed class EventSourcedIntentRepository(FeedDbContext db) : IIntentRepository, IIntentQueryStore
{
    public void Add(Intent intent) => Append(intent);

    public void Update(Intent intent) => Append(intent);

    public Intent? Get(Guid id)
    {
        var stream = db.IntentEvents
            .AsNoTracking()
            .Where(e => e.StreamId == id)
            .OrderBy(e => e.Sequence)
            .ToList();

        return stream.Count == 0 ? null : Intent.Load(id, stream);
    }

    public Page<IntentView> GetPage(Guid communityId, DateTime? before, int limit)
    {
        var query = db.IntentViews.AsNoTracking().Where(v => v.CommunityId == communityId);

        if (before is not null)
            query = query.Where(v => v.DateCreated < before);

        var items = query
            .OrderByDescending(v => v.DateCreated)
            .Take(limit)
            .ToList();

        return new Page<IntentView>(items, items.Count < limit ? null : items[^1].DateCreated);
    }

    public IReadOnlyList<IntentView> GetDueViews(Guid communityId, DateTime now) =>
        db.IntentViews
            .AsNoTracking()
            .Where(v => v.CommunityId == communityId && v.Status == IntentStatus.Open && v.Deadline <= now)
            .ToList();

    public IReadOnlyList<IntentView> GetOpenViewsByTarget(Guid communityId, Guid targetMembershipId) =>
        db.IntentViews
            .AsNoTracking()
            .Where(v => v.CommunityId == communityId
                        && v.TargetMembershipId == targetMembershipId
                        && v.Status == IntentStatus.Open)
            .ToList();

    private void Append(Intent intent)
    {
        if (intent.NewEvents.Count == 0)
            return;

        db.IntentEvents.AddRange(intent.NewEvents);

        var view = db.IntentViews.FirstOrDefault(v => v.Id == intent.Id);
        if (view is null)
            db.IntentViews.Add(new IntentView(intent));
        else
            view.Update(intent);

        db.SaveChanges();
        intent.ClearNewEvents();
    }
}
