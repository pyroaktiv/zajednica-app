using Zajednica.BuildingBlocks.Core.Domain.EventSourcing;

namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class IntentEvent : SourceEvent
{
    protected IntentEvent() { }

    protected IntentEvent(DateTime at)
    {
        OccurredAt = at;
    }
}
