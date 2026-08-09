using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Initiatives;

public sealed class UserTargetingInitiative : Initiative
{
    public UserActionKind Kind { get; }
    public MemberStandingContext Target { get; }

    public Guid TargetMembershipId => Target.MembershipId;

    public UserTargetingInitiative(UserActionKind kind, MemberStandingContext target, Guid communityId,
        Guid authorMembershipId, int eligibleVoterCount, string description)
        : base(communityId, authorMembershipId, eligibleVoterCount, description)
    {
        Kind = kind;
        Target = target;

        EnsureValidTarget();
    }

    public override string KindName => Kind.ToString();

    public override bool AreVotesPublic => Kind is UserActionKind.ManagerElection;

    public override IntentOpened ToOpenedEvent(DateTime now) => new UserTargetingIntentOpened(this, now);

    private void EnsureValidTarget()
    {
        if (Target.Status == MembershipStatus.Unknown)
            throw new EntityValidationException("An initiative has to say what it is about.");
        if (AuthorMembershipId == Target.MembershipId)
            throw new EntityValidationException("An initiative cannot be started by the member it is about.");
        if (Kind == UserActionKind.Ban && Target.Status == MembershipStatus.Banned)
            throw new EntityValidationException("This member is already banned.");
        if (Kind == UserActionKind.ManagerElection && Target.Role == MembershipRole.Manager)
            throw new EntityValidationException("This member is already the manager.");
        if (Target.Status != MembershipStatus.Confirmed)
            throw new EntityValidationException("An initiative can only be started about a confirmed member.");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var component in base.GetEqualityComponents())
            yield return component;

        yield return Kind;
        yield return Target;
    }
}
