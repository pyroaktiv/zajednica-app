namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed record IntentClosed(DateTime OccurredAt, IntentStatus Status, bool Accepted) : IntentEvent(OccurredAt);
