namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class IntentAction
{
    public abstract string Name { get; }

    public abstract void EnsureValidFor(IntentContext context);

    public abstract IntentOpened ToOpenedEvent(IntentContext context, string text);
}
