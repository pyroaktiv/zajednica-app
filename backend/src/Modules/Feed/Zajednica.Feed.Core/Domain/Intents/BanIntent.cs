namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class BanIntent : UserTargetingIntent
{
    internal BanIntent() { }

    public override IntentKind Kind => IntentKind.Ban;

    public static BanIntent Open(Guid communityId, Guid authorMembershipId, Guid targetMembershipId, string text,
        int eligibleVoterCount, bool targetIsConfirmedMember, DateTime now)
    {
        var intent = new BanIntent();
        intent.RaiseOpenedAbout(
            UserTargetingIntentEvent.Opened(intent.Kind, communityId, authorMembershipId, targetMembershipId, text,
                eligibleVoterCount, now),
            targetIsConfirmedMember);

        return intent;
    }
}
