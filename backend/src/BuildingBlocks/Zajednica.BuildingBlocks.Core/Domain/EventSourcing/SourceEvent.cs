namespace Zajednica.BuildingBlocks.Core.Domain.EventSourcing;

public abstract class SourceEvent
{
    public Guid StreamId { get; private set; }
    public int Sequence { get; private set; }
    public DateTime OccurredAt { get; protected set; }

    public void Assign(Guid streamId, int sequence)
    {
        StreamId = streamId;
        Sequence = sequence;
    }
}
