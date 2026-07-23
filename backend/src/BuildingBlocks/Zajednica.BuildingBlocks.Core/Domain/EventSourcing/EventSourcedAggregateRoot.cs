namespace Zajednica.BuildingBlocks.Core.Domain.EventSourcing;

public abstract class EventSourcedAggregateRoot<TEvent> : AggregateRoot where TEvent : SourceEvent
{
    private readonly List<TEvent> _newEvents = [];

    public int Version { get; private set; }
    public IReadOnlyList<TEvent> NewEvents => _newEvents;

    public void ClearNewEvents() => _newEvents.Clear();

    protected void ReplayFromHistory(Guid id, IReadOnlyList<TEvent> history)
    {
        Id = id;

        foreach (var sourceEvent in history)
            ApplyToSelf(sourceEvent);

        Version = history.Count;
    }

    protected void RegisterEvent(TEvent sourceEvent)
    {
        sourceEvent.PlaceInStream(Id, Version + 1);
        ApplyToSelf(sourceEvent);
        Version++;
        _newEvents.Add(sourceEvent);
    }

    protected abstract void ApplyToSelf(TEvent sourceEvent);
}
