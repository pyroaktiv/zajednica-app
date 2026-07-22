namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed record VoteCast(DateTime OccurredAt, Guid VoterMembershipId, bool Value) : IntentEvent(OccurredAt);
