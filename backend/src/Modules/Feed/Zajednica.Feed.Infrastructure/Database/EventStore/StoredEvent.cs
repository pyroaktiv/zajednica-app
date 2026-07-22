namespace Zajednica.Feed.Infrastructure.Database.EventStore;

public sealed class StoredEvent
{
    public Guid StreamId { get; init; }
    public int Sequence { get; init; }
    public string EventType { get; init; } = null!;
    public string Payload { get; init; } = null!;
    public DateTime OccurredAt { get; init; }
}
