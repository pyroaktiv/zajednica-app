using Zajednica.BuildingBlocks.Core.Domain.EventSourcing;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class Intent : EventSourcedAggregateRoot<IntentEvent>
{
    public static readonly TimeSpan VotingWindow = TimeSpan.FromHours(48);

    private readonly Dictionary<Guid, bool> _votes = [];

    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTime DateCreated { get; private set; }
    public DateTime? DateOfClosure { get; private set; }
    public int EligibleVoterCount { get; private set; }
    public IntentStatus Status { get; private set; }

    public abstract IntentKind Kind { get; }

    public DateTime Deadline => DateCreated.Add(VotingWindow);
    public int VotesFor => _votes.Count(v => v.Value);
    public int VotesAgainst => _votes.Count(v => !v.Value);

    protected Intent() { }

    public static Intent Load(Guid id, IReadOnlyList<IntentEvent> history)
    {
        if (history.Count == 0)
            throw new EntityValidationException("An intent stream cannot be empty.");

        Intent intent = history[0].Kind switch
        {
            IntentKind.Ban => new BanIntent(),
            IntentKind.ManagerElection => new ManagerElectionIntent(),
            _ => throw new EntityValidationException("Unknown intent kind.")
        };

        intent.LoadFromHistory(id, history);

        return intent;
    }

    public void CastVote(Guid voterMembershipId, bool inFavor, DateTime now)
    {
        if (Status != IntentStatus.Open)
            throw new EntityValidationException("Voting on this intent is closed.");
        if (_votes.ContainsKey(voterMembershipId))
            throw new EntityValidationException("This member has already voted on this intent.");

        Raise(NewVote(voterMembershipId, inFavor, now));
    }

    public IntentStatus Close(DateTime now)
    {
        if (Status != IntentStatus.Open)
            throw new EntityValidationException("Intent is already closed.");

        var status = Decide();
        Raise(NewClosed(status, now));

        return status;
    }

    public void Cancel(DateTime now)
    {
        if (Status != IntentStatus.Open)
            throw new EntityValidationException("Only an open intent can be cancelled.");

        Raise(NewClosed(IntentStatus.Rejected, now));
    }

    public bool? VoteOf(Guid membershipId) => _votes.TryGetValue(membershipId, out var vote) ? vote : null;

    public bool QuorumReached() => _votes.Count >= EligibleVoterCount / 2 + 1;

    public bool HasDecisiveMajority() => IsThreeQuarters(VotesFor) || IsThreeQuarters(VotesAgainst);

    public bool ShouldClose(DateTime now) => Status == IntentStatus.Open && (now >= Deadline || HasDecisiveMajority());

    protected abstract IntentEvent NewVote(Guid voterMembershipId, bool inFavor, DateTime at);

    protected abstract IntentEvent NewClosed(IntentStatus status, DateTime at);

    protected void RaiseOpened(IntentEvent opened)
    {
        if (string.IsNullOrEmpty(opened.Text))
            throw new EntityValidationException("Text is required.");
        if (opened.EligibleVoterCount < 1)
            throw new EntityValidationException("An intent needs at least one eligible voter.");

        Raise(opened);
    }

    protected override void Apply(IntentEvent intentEvent)
    {
        switch (intentEvent.Type)
        {
            case IntentEventType.Opened:
                CommunityId = intentEvent.CommunityId;
                AuthorMembershipId = intentEvent.AuthorMembershipId;
                Text = intentEvent.Text;
                DateCreated = intentEvent.OccurredAt;
                EligibleVoterCount = intentEvent.EligibleVoterCount;
                Status = IntentStatus.Open;
                break;

            case IntentEventType.VoteCast:
                _votes[intentEvent.VoterMembershipId] = intentEvent.InFavor;
                break;

            case IntentEventType.Closed:
                Status = intentEvent.Status;
                DateOfClosure = intentEvent.OccurredAt;
                break;
        }
    }

    private IntentStatus Decide()
    {
        if (!QuorumReached())
            return IntentStatus.Expired;

        return VotesFor > VotesAgainst ? IntentStatus.Accepted : IntentStatus.Rejected;
    }

    private bool IsThreeQuarters(int votes) => votes * 4 >= EligibleVoterCount * 3;
}
