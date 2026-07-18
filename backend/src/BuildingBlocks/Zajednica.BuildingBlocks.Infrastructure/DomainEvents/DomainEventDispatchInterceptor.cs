using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.BuildingBlocks.Infrastructure.DomainEvents;

public sealed class DomainEventDispatchInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        if (eventData.Context is not null)
            await DispatchAsync(eventData.Context, ct);

        return await base.SavedChangesAsync(eventData, result, ct);
    }

    private async Task DispatchAsync(DbContext context, CancellationToken ct)
    {
        var roots = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        // Collect and clear first: a handler may itself SaveChanges and re-enter here.
        var events = roots.SelectMany(r => r.DomainEvents).ToList();
        roots.ForEach(r => r.ClearDomainEvents());

        foreach (var domainEvent in events)
            await dispatcher.DispatchAsync(domainEvent, ct);
    }
}
