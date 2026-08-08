using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed class UserTargetingIntentOpened : IntentOpened
{
    public UserActionKind Kind { get; private set; }
    public Guid TargetMembershipId { get; private set; }
    public MembershipStatus TargetMembershipStatus { get; private set; }
    public MembershipRole TargetMembershipRole { get; private set; }

    private UserTargetingIntentOpened() { }

    public UserTargetingIntentOpened(UserTargetingInitiative initiative, DateTime now) : base(initiative, now)
    {
        Kind = initiative.Kind;
        TargetMembershipId = initiative.TargetMembershipId;
        TargetMembershipStatus = initiative.TargetMembershipStatus;
        TargetMembershipRole = initiative.TargetMembershipRole;
    }

    public override Initiative ToInitiative() =>
        new UserTargetingInitiative(Kind, TargetMembershipId, TargetMembershipStatus, TargetMembershipRole,
            CommunityId, AuthorMembershipId, EligibleVoterCount, Description);
}
