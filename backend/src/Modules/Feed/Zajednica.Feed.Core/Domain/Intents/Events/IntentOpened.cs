using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public abstract class IntentOpened : IntentEvent
{
    public Guid CommunityId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int EligibleVoterCount { get; private set; }

    protected IntentOpened() { }

    protected IntentOpened(Initiative initiative, DateTime now) : base(now)
    {
        CommunityId = initiative.CommunityId;
        AuthorMembershipId = initiative.AuthorMembershipId;
        Description = initiative.Description;
        EligibleVoterCount = initiative.EligibleVoterCount;
    }

    public abstract Initiative ToInitiative();
}
