namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class IntentAction
{
    public abstract string Name { get; }

    public abstract void EnsureValidFor(ActionContext context);

    public abstract IntentOpened ToOpenedEvent(ActionContext context, string text);
}
