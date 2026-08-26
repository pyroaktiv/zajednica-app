using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Intents.Initiatives;

public abstract class UserTargetingInitiative : Initiative
{
    public MemberStandingContext Target { get; }

    public Guid TargetMembershipId => Target.MembershipId;

    protected UserTargetingInitiative(MemberStandingContext target, Guid communityId, Guid authorMembershipId,
        int eligibleVoterCount, string description)
        : base(communityId, authorMembershipId, eligibleVoterCount, description)
    {
        Target = target;

        EnsureValidTarget();
    }

    public override bool AreVotesPublic => false;

    public override bool Supersedes(Initiative other) =>
        other is UserTargetingInitiative o
        && o.CommunityId == CommunityId
        && o.TargetMembershipId == TargetMembershipId
        && other.GetType() == GetType();

    protected virtual void EnsureSpecificTarget()
    {
    }

    private void EnsureValidTarget()
    {
        if (Target.Status == MembershipStatus.Unknown)
            throw new EntityValidationException("An initiative has to say what it is about.");
        if (AuthorMembershipId == Target.MembershipId)
            throw new EntityValidationException("An initiative cannot be started by the member it is about.");

        EnsureSpecificTarget();

        if (Target.Status != MembershipStatus.Confirmed)
            throw new EntityValidationException("An initiative can only be started about a confirmed member.");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var component in base.GetEqualityComponents())
            yield return component;

        yield return Target;
    }
}
