using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Actions;

public abstract class IntentAction : ValueObject
{
    public IntentContext Context { get; }
    
    public abstract string Name { get; }

    protected IntentAction(IntentContext context) => Context = context;

    public abstract IntentOpened ToOpenedEvent(string text);
}
