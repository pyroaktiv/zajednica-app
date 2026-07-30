using Zajednica.Feed.Core.Domain.Intents.Actions;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public abstract class IntentOpened : IntentEvent
{
    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public int EligibleVoterCount { get; private set; }

    protected IntentOpened() { }
    
    protected IntentOpened(IntentContext context, string text) : base(context.Now)
    {
        CommunityId = context.CommunityId;
        AuthorMembershipId = context.AuthorMembershipId;
        Text = text.Trim();
        EligibleVoterCount = context.EligibleVoterCount;
    }

    public abstract IntentAction ToAction();

    protected IntentContext.Builder ToContextBuilder() =>
        new IntentContext.Builder()
            .WithCommunityId(CommunityId)
            .WithAuthorMembershipId(AuthorMembershipId)
            .WithEligibleVoterCount(EligibleVoterCount)
            .At(OccurredAt);
}
