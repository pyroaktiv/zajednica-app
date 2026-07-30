using Zajednica.Feed.Core.Domain.Intents.Actions;

namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed class UserTargetingIntentOpened : IntentOpened
{
    public UserActionKind Kind { get; private set; }
    public Guid TargetMembershipId { get; private set; }
    public MembershipStatus TargetMembershipStatus { get; private set; }

    private UserTargetingIntentOpened() { }

    public UserTargetingIntentOpened(UserActionKind kind, Guid targetMembershipId, IntentContext context, string text)
        : base(context, text)
    {
        Kind = kind;
        TargetMembershipId = targetMembershipId;
        TargetMembershipStatus = context.TargetMembershipStatus;
    }

    public override IntentAction ToAction() =>
        new UserTargetingAction(Kind, TargetMembershipId,
            ToContextBuilder().WithTargetMembershipStatus(TargetMembershipStatus).Build());
}
