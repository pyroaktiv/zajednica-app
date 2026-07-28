using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class UserTargetingAction : IntentAction
{
    public UserActionKind Kind { get; }
    public Guid TargetMembershipId { get; }

    public override string Name => Kind.ToString();

    internal UserTargetingAction(UserActionKind kind, Guid targetMembershipId)
    {
        Kind = kind;
        TargetMembershipId = targetMembershipId;
    }

    public static UserTargetingAction For(UserActionKind kind, Guid targetMembershipId, bool targetIsConfirmedMember)
    {
        if (!targetIsConfirmedMember)
            throw new EntityValidationException("An intent can only be opened about a confirmed member.");

        return new UserTargetingAction(kind, targetMembershipId);
    }

    public override IntentOpened ToOpenedEvent(Guid communityId, Guid authorMembershipId, string text,
        int eligibleVoterCount, DateTime at) =>
        new UserTargetingIntentOpened(Kind, TargetMembershipId, communityId, authorMembershipId, text,
            eligibleVoterCount, at);
}
