using Zajednica.BuildingBlocks.Core.Domain.EventSourcing;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Intents;

public class Intent : EventSourcedAggregateRoot<IntentEvent>
{
    private static readonly TimeSpan VotingWindow = TimeSpan.FromHours(48);

    private readonly Dictionary<Guid, bool> _votes = [];

    public IntentAction Action { get; private set; }
    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTime DateCreated { get; private set; }
    public DateTime? DateOfClosure { get; private set; }
    public int EligibleVoterCount { get; private set; }
    public IntentStatus Status { get; private set; }

    public DateTime Deadline => DateCreated.Add(VotingWindow);
    public int VotesFor => _votes.Count(v => v.Value);
    public int VotesAgainst => _votes.Count(v => !v.Value);

    private Intent() { }

    public static Intent Open(IntentAction action, Guid communityId, Guid authorMembershipId, string text,
        int eligibleVoterCount, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new EntityValidationException("Text is required.");
        if (eligibleVoterCount < 2)
            throw new EntityValidationException("An intent needs at least two eligible voters.");

        var context = new IntentContext(communityId, authorMembershipId, eligibleVoterCount, now);
        action.EnsureValidFor(context);

        var intent = new Intent();
        intent.RegisterEvent(action.ToOpenedEvent(context, text));

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

    public bool QuorumReached() => _votes.Count >= EligibleVoterCount / 2 + 1;

    public bool HasDecisiveMajority() => IsThreeQuarters(VotesFor) || IsThreeQuarters(VotesAgainst);

    public bool ShouldClose(DateTime now) => Status == IntentStatus.Open && (now >= Deadline || HasDecisiveMajority());

    protected override void ApplyToSelf(IntentEvent intentEvent)
    {
        switch (intentEvent)
        {
            case IntentOpened opened:
                Action = opened.ToAction();
                CommunityId = opened.CommunityId;
                AuthorMembershipId = opened.AuthorMembershipId;
                Text = opened.Text;
                DateCreated = opened.OccurredAt;
                EligibleVoterCount = opened.EligibleVoterCount;
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

    private bool IsThreeQuarters(int votes) => votes * 4 >= EligibleVoterCount * 3;
}
