using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class IntentContext : ValueObject
{
    public Guid CommunityId { get; }
    public Guid AuthorMembershipId { get; }
    public int EligibleVoterCount { get; }
    public DateTime Now { get; }

    public IntentContext(Guid communityId, Guid authorMembershipId, int eligibleVoterCount, DateTime now)
    {
        CommunityId = communityId;
        AuthorMembershipId = authorMembershipId;
        EligibleVoterCount = eligibleVoterCount;
        Now = now;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CommunityId;
        yield return AuthorMembershipId;
        yield return EligibleVoterCount;
        yield return Now;
    }
}
