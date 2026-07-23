namespace Zajednica.BuildingBlocks.Core.Domain.EventSourcing;

public abstract class EventSourcedAggregateRoot<TEvent> : AggregateRoot where TEvent : SourceEvent
{
    private readonly List<TEvent> _newEvents = [];

    public int Version { get; private set; }
    public IReadOnlyList<TEvent> NewEvents => _newEvents;

    public void ClearNewEvents() => _newEvents.Clear();

    protected void LoadFromHistory(Guid id, IReadOnlyList<TEvent> history)
    {
        Id = id;

        foreach (var sourceEvent in history)
            Apply(sourceEvent);

        Version = history.Count;
    }

    protected void Raise(TEvent sourceEvent)
    {
        sourceEvent.PlaceInStream(Id, Version + 1);
        Apply(sourceEvent);
        Version++;
        _newEvents.Add(sourceEvent);
    }

    protected abstract void Apply(TEvent sourceEvent);
}
