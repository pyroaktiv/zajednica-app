using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.UseCases.Queries;

public class IntentView
{
    public Guid Id { get; private set; }
    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public Guid? TargetMembershipId { get; private set; }
    public IntentKind Kind { get; private set; }
    public string Text { get; private set; } = null!;
    public DateTime DateCreated { get; private set; }
    public DateTime Deadline { get; private set; }
    public DateTime? DateOfClosure { get; private set; }
    public IntentStatus Status { get; private set; }
    public int EligibleVoterCount { get; private set; }
    public int VotesFor { get; private set; }
    public int VotesAgainst { get; private set; }

    private IntentView() { }

    public IntentView(Intent intent)
    {
        Id = intent.Id;
        CommunityId = intent.CommunityId;
        AuthorMembershipId = intent.AuthorMembershipId;
        TargetMembershipId = (intent as UserTargetingIntent)?.TargetMembershipId;
        Kind = intent.Kind;
        Text = intent.Text;
        DateCreated = intent.DateCreated;
        Deadline = intent.Deadline;
        EligibleVoterCount = intent.EligibleVoterCount;

        Update(intent);
    }

    public void Update(Intent intent)
    {
        Status = intent.Status;
        DateOfClosure = intent.DateOfClosure;
        VotesFor = intent.VotesFor;
        VotesAgainst = intent.VotesAgainst;
    }
}
