using Zajednica.BuildingBlocks.Core.Domain.EventSourcing;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class Intent : EventSourcedAggregate
{
    public static readonly TimeSpan VotingWindow = TimeSpan.FromHours(48);

    private readonly List<VoteCast> _votes = [];

    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public Guid TargetMembershipId { get; private set; }
    public string Text { get; private set; } = null!;
    public DateTime DateCreated { get; private set; }
    public IntentStatus Status { get; private set; }
    public DateTime Deadline { get; private set; }
    public DateTime? DateOfClosure { get; private set; }
    public int EligibleVoterCount { get; private set; }
    public IReadOnlyList<VoteCast> Votes => _votes;

    public abstract string IntentType { get; }

    public int VotesFor => _votes.Count(v => v.Value);
    public int VotesAgainst => _votes.Count(v => !v.Value);

    protected Intent() { }

    public static Intent Rehydrate(Guid id, IReadOnlyList<IntentEvent> history)
    {
        if (history.Count == 0)
            throw new EntityValidationException("An intent stream cannot be empty.");

        Intent intent = history[0] switch
        {
            BanIntentOpened => new BanIntent(),
            ManagerElectionIntentOpened => new ManagerElectionIntent(),
            _ => throw new EntityValidationException("An intent stream must start with an opening event.")
        };

        intent.LoadFromHistory(id, history);
        return intent;
    }

    public void CastVote(Guid voterMembershipId, bool value, DateTime now)
    {
        if (Status != IntentStatus.Open)
            throw new EntityValidationException("Voting on this intent is closed.");
        if (voterMembershipId == Guid.Empty)
            throw new EntityValidationException("VoterMembershipId is required.");
        if (_votes.Any(v => v.VoterMembershipId == voterMembershipId))
            throw new EntityValidationException("This member has already voted on this intent.");

        Raise(new VoteCast(now, voterMembershipId, value));
    }

    public bool QuorumReached() => _votes.Count >= EligibleVoterCount / 2 + 1;

    public bool ShouldAutoClose() => ReachedDecisiveShare(VotesFor) || ReachedDecisiveShare(VotesAgainst);

    public bool ShouldClose(DateTime now) => Status == IntentStatus.Open && (now >= Deadline || ShouldAutoClose());

    public IntentOutcome Close(DateTime now)
    {
        if (Status != IntentStatus.Open)
            throw new EntityValidationException("Intent is already closed.");

        var status = Decide();
        Raise(new IntentClosed(now, status, status == IntentStatus.Accepted));

        return new IntentOutcome(status == IntentStatus.Accepted, status, now);
    }

    public void Cancel(DateTime now)
    {
        if (Status != IntentStatus.Open)
            throw new EntityValidationException("Only an open intent can be cancelled.");

        Raise(new IntentCancelled(now));
    }

    protected static TOpened Validated<TOpened>(TOpened opened, bool targetEligible) where TOpened : IntentOpened
    {
        if (opened.CommunityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");
        if (opened.AuthorMembershipId == Guid.Empty)
            throw new EntityValidationException("AuthorMembershipId is required.");
        if (opened.TargetMembershipId == Guid.Empty)
            throw new EntityValidationException("TargetMembershipId is required.");
        if (opened.TargetMembershipId == opened.AuthorMembershipId)
            throw new EntityValidationException("An intent cannot be opened about its own author.");
        if (!targetEligible)
            throw new EntityValidationException("An intent can only be opened about a confirmed member of the community.");
        if (string.IsNullOrWhiteSpace(opened.Text))
            throw new EntityValidationException("Text is required.");
        if (opened.EligibleVoterCount < 1)
            throw new EntityValidationException("An intent needs at least one eligible voter.");

        return opened;
    }

    protected override void Apply(DomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case IntentOpened opened:
                ApplyOpened(opened);
                break;
            case VoteCast vote:
                _votes.Add(vote);
                break;
            case IntentClosed closed:
                Status = closed.Status;
                DateOfClosure = closed.OccurredAt;
                break;
            case IntentCancelled cancelled:
                Status = IntentStatus.Rejected;
                DateOfClosure = cancelled.OccurredAt;
                break;
        }
    }

    private void ApplyOpened(IntentOpened opened)
    {
        CommunityId = opened.CommunityId;
        AuthorMembershipId = opened.AuthorMembershipId;
        TargetMembershipId = opened.TargetMembershipId;
        Text = opened.Text;
        DateCreated = opened.OccurredAt;
        Deadline = opened.Deadline;
        EligibleVoterCount = opened.EligibleVoterCount;
        Status = IntentStatus.Open;
    }

    private IntentStatus Decide()
    {
        if (!QuorumReached())
            return IntentStatus.Expired;

        return VotesFor > VotesAgainst ? IntentStatus.Accepted : IntentStatus.Rejected;
    }

    private bool ReachedDecisiveShare(int votes) => votes * 4 >= EligibleVoterCount * 3;
}
