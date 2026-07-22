using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.BuildingBlocks.Infrastructure.Database;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Infrastructure.Database.EventStore;

namespace Zajednica.Feed.Infrastructure.Database.Repositories;

internal sealed class EventSourcedIntentRepository(FeedDbContext db) : IIntentRepository
{
    public Task AddAsync(Intent intent, CancellationToken ct = default) => AppendAsync(intent, ct);

    public Task UpdateAsync(Intent intent, CancellationToken ct = default) => AppendAsync(intent, ct);

    public async Task<Intent?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var stream = await db.IntentEvents
            .AsNoTracking()
            .Where(e => e.StreamId == id)
            .OrderBy(e => e.Sequence)
            .ToListAsync(ct);

        if (stream.Count == 0)
            return null;

        return Intent.Rehydrate(id, stream.Select(e => IntentEventSerializer.Deserialize(e.Payload)).ToList());
    }

    public Task<PagedResult<IntentView>> GetPagedViewsAsync(Guid communityId, int page, int pageSize, CancellationToken ct = default) =>
        db.IntentViews
            .AsNoTracking()
            .Where(v => v.CommunityId == communityId)
            .OrderByDescending(v => v.DateCreated)
            .GetPaged(page, pageSize);

    public async Task<IReadOnlyList<IntentView>> GetDueViewsAsync(Guid communityId, DateTime now, CancellationToken ct = default) =>
        await db.IntentViews
            .AsNoTracking()
            .Where(v => v.CommunityId == communityId && v.Status == IntentStatus.Open && v.Deadline <= now)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<IntentView>> GetOpenViewsByTargetAsync(Guid communityId, Guid targetMembershipId,
        CancellationToken ct = default) =>
        await db.IntentViews
            .AsNoTracking()
            .Where(v => v.CommunityId == communityId
                        && v.TargetMembershipId == targetMembershipId
                        && v.Status == IntentStatus.Open)
            .ToListAsync(ct);

    private async Task AppendAsync(Intent intent, CancellationToken ct)
    {
        var pending = intent.DequeuePendingEvents().Cast<IntentEvent>().ToList();
        if (pending.Count == 0)
            return;

        var baseSequence = intent.Version - pending.Count;
        for (var i = 0; i < pending.Count; i++)
            db.IntentEvents.Add(new StoredEvent
            {
                StreamId = intent.Id,
                Sequence = baseSequence + i + 1,
                EventType = pending[i].GetType().Name,
                Payload = IntentEventSerializer.Serialize(pending[i]),
                OccurredAt = pending[i].OccurredAt
            });

        await IntentViewProjector.ProjectAsync(db, intent, ct);
        await db.SaveChangesAsync(ct);
    }
}
