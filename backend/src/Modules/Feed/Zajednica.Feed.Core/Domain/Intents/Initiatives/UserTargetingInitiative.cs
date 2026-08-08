using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Initiatives;

public sealed class UserTargetingInitiative : Initiative
{
    public UserActionKind Kind { get; }
    public Guid TargetMembershipId { get; }
    public MembershipStatus TargetMembershipStatus { get; }
    public MembershipRole TargetMembershipRole { get; }

    public UserTargetingInitiative(UserActionKind kind, Guid targetMembershipId,
        MembershipStatus targetMembershipStatus, MembershipRole targetMembershipRole, Guid communityId,
        Guid authorMembershipId, int eligibleVoterCount, string description)
        : base(communityId, authorMembershipId, eligibleVoterCount, description)
    {
        Kind = kind;
        TargetMembershipId = targetMembershipId;
        TargetMembershipStatus = targetMembershipStatus;
        TargetMembershipRole = targetMembershipRole;

        EnsureValidTarget();
    }

    public override string KindName => Kind.ToString();

    public override bool AreVotesPublic => Kind is UserActionKind.ManagerElection;

    public override IntentOpened ToOpenedEvent(DateTime now) => new UserTargetingIntentOpened(this, now);

    private void EnsureValidTarget()
    {
        if (TargetMembershipStatus == MembershipStatus.Unknown)
            throw new EntityValidationException("An initiative has to say what it is about.");
        if (AuthorMembershipId == TargetMembershipId)
            throw new EntityValidationException("An initiative cannot be started by the member it is about.");
        if (Kind == UserActionKind.Ban && TargetMembershipStatus == MembershipStatus.Banned)
            throw new EntityValidationException("This member is already banned.");
        if (Kind == UserActionKind.ManagerElection && TargetMembershipRole == MembershipRole.Manager)
            throw new EntityValidationException("This member is already the manager.");
        if (TargetMembershipStatus != MembershipStatus.Confirmed)
            throw new EntityValidationException("An initiative can only be started about a confirmed member.");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var component in base.GetEqualityComponents())
            yield return component;

        yield return Kind;
        yield return TargetMembershipId;
        yield return TargetMembershipStatus;
        yield return TargetMembershipRole;
    }
}
