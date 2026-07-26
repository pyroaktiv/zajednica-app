using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.BuildingBlocks.Core.Domain.EventSourcing;

public abstract class EventSourcedAggregateRoot<TEvent> : AggregateRoot where TEvent : SourceEvent
{
    private readonly List<TEvent> _newEvents = [];

    public int Version { get; private set; }
    public IReadOnlyList<TEvent> NewEvents => _newEvents;

    public void ClearNewEvents() => _newEvents.Clear();

    protected void ReplayFromHistory(IReadOnlyList<TEvent> history)
    {
        if (history.Count == 0)
            throw new EntityValidationException("An event stream cannot be empty.");

        Id = history[0].StreamId;

        foreach (var sourceEvent in history)
        {
            if (sourceEvent.StreamId != Id)
                throw new EntityValidationException("All events must belong to the same stream.");

            ApplyToSelf(sourceEvent);
        }

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
