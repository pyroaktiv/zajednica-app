using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class UserTargetingIntent : Intent
{
    public Guid TargetMembershipId { get; private set; }

    protected UserTargetingIntent() { }

    protected void RaiseOpenedAbout(UserTargetingIntentEvent opened, bool targetIsConfirmedMember)
    {
        if (opened.TargetMembershipId == opened.AuthorMembershipId)
            throw new EntityValidationException("An intent cannot be opened about its own author.");
        if (!targetIsConfirmedMember)
            throw new EntityValidationException("An intent can only be opened about a confirmed member of the community.");

        RaiseOpened(opened);
    }

    protected override IntentEvent NewVote(Guid voterMembershipId, bool inFavor, DateTime at) =>
        UserTargetingIntentEvent.Vote(voterMembershipId, inFavor, at);

    protected override IntentEvent NewClosed(IntentStatus status, DateTime at) =>
        UserTargetingIntentEvent.Closed(status, at);

    protected override void Apply(IntentEvent intentEvent)
    {
        base.Apply(intentEvent);

        if (intentEvent is UserTargetingIntentEvent { Type: IntentEventType.Opened } opened)
            TargetMembershipId = opened.TargetMembershipId;
    }
}
