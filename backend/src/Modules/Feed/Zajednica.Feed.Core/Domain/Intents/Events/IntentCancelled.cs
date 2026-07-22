namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed record IntentCancelled(DateTime OccurredAt) : IntentEvent(OccurredAt);
