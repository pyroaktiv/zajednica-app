using Zajednica.BuildingBlocks.Core.Domain.EventSourcing;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents;

public class Intent : EventSourcedAggregateRoot<IntentEvent>
{
    private static readonly TimeSpan VotingWindow = TimeSpan.FromHours(48);

    private readonly Dictionary<Guid, bool> _votes = [];

    public Initiative Initiative { get; private set; }
    public DateTime DateCreated { get; private set; }
    public DateTime? DateOfClosure { get; private set; }
    public IntentStatus Status { get; private set; }

    public DateTime Deadline => DateCreated.Add(VotingWindow);
    public int VotesFor => _votes.Count(v => v.Value);
    public int VotesAgainst => _votes.Count(v => !v.Value);

    private Intent() { }

    public static Intent Open(Initiative initiative, DateTime now)
    {
        var intent = new Intent();
        intent.RegisterEvent(initiative.ToOpenedEvent(now));

        return intent;
    }

    public static Intent Load(IReadOnlyList<IntentEvent> history)
    {
        if (history.Count == 0)
            throw new EntityValidationException("An intent stream cannot be empty.");
        if (history[0] is not IntentOpened)
            throw new EntityValidationException("An intent stream must begin with the event that opened it.");

        var intent = new Intent();
        intent.ReplayFromHistory(history);

        return intent;
    }

    public void CastVote(Guid voterMembershipId, bool inFavor, DateTime now)
    {
        if (Status != IntentStatus.Open || now >= Deadline)
            throw new EntityValidationException("Voting on this intent is closed.");
        if (_votes.ContainsKey(voterMembershipId))
            throw new EntityValidationException("This member has already voted on this intent.");

        RegisterEvent(new VoteCast(voterMembershipId, inFavor, now));
    }

    public IntentStatus Close(DateTime now)
    {
        if (Status != IntentStatus.Open)
            throw new EntityValidationException("Intent is already closed.");

        var status = Decide();
        RegisterEvent(new IntentClosed(status, ClosureReason.Decision, now));

        return status;
    }

    public void Supersede(DateTime now)
    {
        if (Status != IntentStatus.Open)
            throw new EntityValidationException("Only an open intent can be superseded.");

        RegisterEvent(new IntentClosed(IntentStatus.Rejected, ClosureReason.Superseded, now));
    }

    public bool? VoteOf(Guid membershipId) => _votes.TryGetValue(membershipId, out var vote) ? vote : null;

    public bool QuorumReached() => _votes.Count >= Initiative.EligibleVoterCount / 2 + 1;

    public bool HasDecisiveMajority() => IsThreeQuarters(VotesFor) || IsThreeQuarters(VotesAgainst);

    public bool ShouldClose(DateTime now) => Status == IntentStatus.Open && (now >= Deadline || HasDecisiveMajority());

    protected override void ApplyToSelf(IntentEvent intentEvent)
    {
        switch (intentEvent)
        {
            case IntentOpened opened:
                Initiative = opened.ToInitiative();
                DateCreated = opened.OccurredAt;
                Status = IntentStatus.Open;
                break;

            case VoteCast vote:
                _votes[vote.VoterMembershipId] = vote.InFavor;
                break;

            case IntentClosed closed:
                Status = closed.Status;
                DateOfClosure = closed.OccurredAt;
                break;
        }
    }

    private IntentStatus Decide()
    {
        if (!QuorumReached())
            return IntentStatus.Expired;

        return VotesFor > VotesAgainst ? IntentStatus.Accepted : IntentStatus.Rejected;
    }

    private bool IsThreeQuarters(int votes) => votes * 4 >= Initiative.EligibleVoterCount * 3;
}
