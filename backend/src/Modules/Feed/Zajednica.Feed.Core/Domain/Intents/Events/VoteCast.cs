namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class VoteCast : IntentEvent
{
    public Guid VoterMembershipId { get; private set; }
    public bool InFavor { get; private set; }

    private VoteCast() { }

    public VoteCast(Guid voterMembershipId, bool inFavor, DateTime at) : base(at)
    {
        VoterMembershipId = voterMembershipId;
        InFavor = inFavor;
    }
}
