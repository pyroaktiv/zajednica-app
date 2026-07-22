using System.Text.Json.Serialization;
using Zajednica.BuildingBlocks.Core.Domain.EventSourcing;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "eventType")]
[JsonDerivedType(typeof(BanIntentOpened), nameof(BanIntentOpened))]
[JsonDerivedType(typeof(ManagerElectionIntentOpened), nameof(ManagerElectionIntentOpened))]
[JsonDerivedType(typeof(VoteCast), nameof(VoteCast))]
[JsonDerivedType(typeof(IntentClosed), nameof(IntentClosed))]
[JsonDerivedType(typeof(IntentCancelled), nameof(IntentCancelled))]
public abstract record IntentEvent(DateTime OccurredAt) : DomainEvent(OccurredAt);
