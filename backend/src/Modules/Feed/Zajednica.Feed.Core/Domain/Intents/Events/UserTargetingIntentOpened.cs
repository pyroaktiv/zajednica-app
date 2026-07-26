namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class UserTargetingIntentOpened : IntentOpened
{
    public UserActionKind Kind { get; private set; }
    public Guid TargetMembershipId { get; private set; }

    private UserTargetingIntentOpened() { }

    public UserTargetingIntentOpened(UserActionKind kind, Guid targetMembershipId, Guid communityId,
        Guid authorMembershipId, string text, int eligibleVoterCount, DateTime at)
        : base(communityId, authorMembershipId, text, eligibleVoterCount, at)
    {
        Kind = kind;
        TargetMembershipId = targetMembershipId;
    }

    public override IntentAction ToAction() => new UserTargetingAction(Kind, TargetMembershipId);
}
