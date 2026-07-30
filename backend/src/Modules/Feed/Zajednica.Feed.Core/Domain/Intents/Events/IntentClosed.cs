namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed class IntentClosed : IntentEvent
{
    public IntentStatus Status { get; private set; }
    public ClosureReason Reason { get; private set; }

    private IntentClosed() { }

    public IntentClosed(IntentStatus status, ClosureReason reason, DateTime at) : base(at)
    {
        Status = status;
        Reason = reason;
    }
}
