using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Initiatives;

public abstract class Initiative : ValueObject
{
    public Guid CommunityId { get; }
    public Guid AuthorMembershipId { get; }
    public int EligibleVoterCount { get; }
    public string Description { get; }

    public abstract string KindName { get; }

    public virtual bool AreVotesPublic => true;

    public virtual bool Supersedes(Initiative other) => false;

    protected Initiative(Guid communityId, Guid authorMembershipId, int eligibleVoterCount, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new EntityValidationException("An initiative needs a description.");
        if (authorMembershipId == Guid.Empty)
            throw new EntityValidationException("An initiative has to say who is opening it.");
        if (eligibleVoterCount < 2)
            throw new EntityValidationException("An initiative needs at least two eligible voters.");

        CommunityId = communityId;
        AuthorMembershipId = authorMembershipId;
        EligibleVoterCount = eligibleVoterCount;
        Description = description.Trim();
    }

    public virtual int GetQuorum() => EligibleVoterCount / 2 + 1;
    
    public virtual int GetDecisiveThreshold() => EligibleVoterCount * 3 / 4 + 1;

    public abstract IntentOpened ToOpenedEvent(DateTime now);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CommunityId;
        yield return AuthorMembershipId;
        yield return EligibleVoterCount;
        yield return Description;
    }
}
