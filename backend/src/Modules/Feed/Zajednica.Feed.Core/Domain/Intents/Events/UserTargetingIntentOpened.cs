using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public abstract class UserTargetingIntentOpened : IntentOpened
{
    public Guid TargetMembershipId { get; private set; }
    public MembershipStatus TargetMembershipStatus { get; private set; }
    public MembershipRole TargetMembershipRole { get; private set; }

    protected UserTargetingIntentOpened() { }

    protected UserTargetingIntentOpened(UserTargetingInitiative initiative, DateTime now) : base(initiative, now)
    {
        TargetMembershipId = initiative.Target.MembershipId;
        TargetMembershipStatus = initiative.Target.Status;
        TargetMembershipRole = initiative.Target.Role;
    }

    protected MemberStandingContext TargetStanding() =>
        new(TargetMembershipId, TargetMembershipStatus, TargetMembershipRole);
}
