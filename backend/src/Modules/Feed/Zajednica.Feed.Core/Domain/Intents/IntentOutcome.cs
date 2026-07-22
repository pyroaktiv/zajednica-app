using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.Feed.Core.Domain.Intents;

public class IntentOutcome : ValueObject
{
    public bool Accepted { get; private set; }
    public IntentStatus Status { get; private set; }
    public DateTime DateOfClosure { get; private set; }

    internal IntentOutcome(bool accepted, IntentStatus status, DateTime dateOfClosure)
    {
        Accepted = accepted;
        Status = status;
        DateOfClosure = dateOfClosure;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Accepted;
        yield return Status;
        yield return DateOfClosure;
    }
}
