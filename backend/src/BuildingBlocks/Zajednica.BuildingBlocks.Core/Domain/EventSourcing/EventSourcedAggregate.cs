namespace Zajednica.BuildingBlocks.Core.Domain.EventSourcing;

public abstract class EventSourcedAggregate : AggregateRoot
{
    private readonly List<DomainEvent> _pendingEvents = [];

    public int Version { get; private set; }

    public void LoadFromHistory(Guid id, IEnumerable<DomainEvent> history)
    {
        Id = id;
        foreach (var domainEvent in history)
        {
            Apply(domainEvent);
            Version++;
        }
    }

    public IReadOnlyList<DomainEvent> DequeuePendingEvents()
    {
        var pending = _pendingEvents.ToList();
        _pendingEvents.Clear();
        return pending;
    }

    protected void Raise(DomainEvent domainEvent)
    {
        Apply(domainEvent);
        _pendingEvents.Add(domainEvent);
        Version++;
    }

    protected abstract void Apply(DomainEvent domainEvent);
}
