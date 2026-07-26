namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class IntentAction
{
    public abstract string Name { get; }

    public abstract void EnsureValidFor(Guid authorMembershipId);

    public abstract IntentOpened ToOpenedEvent(Guid communityId, Guid authorMembershipId, string text,
        int eligibleVoterCount, DateTime at);
}
