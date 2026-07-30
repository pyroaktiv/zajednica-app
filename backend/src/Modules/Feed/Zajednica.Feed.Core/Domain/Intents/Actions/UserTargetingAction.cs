using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents.Actions;

public sealed class UserTargetingAction : IntentAction
{
    public UserTargetingAction(UserActionKind kind, Guid targetMembershipId, IntentContext context) : base(context)
    {
        Kind = kind;
        TargetMembershipId = targetMembershipId;
        
        EnsureValidContext();
    }
    
    public UserActionKind Kind { get; }
    public Guid TargetMembershipId { get; }

    public override string Name => Kind.ToString();

    private void EnsureValidContext()
    {
        if (Context.AuthorMembershipId == Guid.Empty)
            throw new EntityValidationException("The context of an intent has to say who is opening it.");
        if (Context.TargetMembershipStatus == MembershipStatus.Unknown)
            throw new EntityValidationException("The context of an intent has to say what the member it is about is.");
        if (Context.TargetMembershipStatus != MembershipStatus.Confirmed)
            throw new EntityValidationException("An intent can only be opened about a confirmed member.");
        if (Context.AuthorMembershipId == TargetMembershipId)
            throw new EntityValidationException("An intent cannot be opened by the member it is about.");
    }

    public override IntentOpened ToOpenedEvent(string text) =>
        new UserTargetingIntentOpened(Kind, TargetMembershipId, Context, text);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Kind;
        yield return TargetMembershipId;
        yield return Context;
    }
}
