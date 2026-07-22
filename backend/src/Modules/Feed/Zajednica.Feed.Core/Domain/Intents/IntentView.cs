using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.Feed.Core.Domain.Intents;

public class IntentView : Entity
{
    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public Guid TargetMembershipId { get; private set; }
    public string IntentType { get; private set; } = null!;
    public string Text { get; private set; } = null!;
    public DateTime DateCreated { get; private set; }
    public IntentStatus Status { get; private set; }
    public DateTime Deadline { get; private set; }
    public DateTime? DateOfClosure { get; private set; }
    public int EligibleVoterCount { get; private set; }
    public int VotesFor { get; private set; }
    public int VotesAgainst { get; private set; }

    private IntentView() { }

    public IntentView(Intent intent)
    {
        Id = intent.Id;
        CommunityId = intent.CommunityId;
        AuthorMembershipId = intent.AuthorMembershipId;
        TargetMembershipId = intent.TargetMembershipId;
        IntentType = intent.IntentType;
        Text = intent.Text;
        DateCreated = intent.DateCreated;
        Deadline = intent.Deadline;
        EligibleVoterCount = intent.EligibleVoterCount;

        Refresh(intent);
    }

    public void Refresh(Intent intent)
    {
        Status = intent.Status;
        DateOfClosure = intent.DateOfClosure;
        VotesFor = intent.VotesFor;
        VotesAgainst = intent.VotesAgainst;
    }
}
