namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class IntentOpened : IntentEvent
{
    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public int EligibleVoterCount { get; private set; }

    protected IntentOpened() { }

    protected IntentOpened(Guid communityId, Guid authorMembershipId, string text, int eligibleVoterCount, DateTime at)
        : base(at)
    {
        CommunityId = communityId;
        AuthorMembershipId = authorMembershipId;
        Text = text.Trim();
        EligibleVoterCount = eligibleVoterCount;
    }

    public abstract IntentAction ToAction();
}
