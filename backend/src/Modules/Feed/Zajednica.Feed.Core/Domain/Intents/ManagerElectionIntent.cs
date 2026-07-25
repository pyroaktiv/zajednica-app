namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class ManagerElectionIntent : UserTargetingIntent
{
    internal ManagerElectionIntent() { }

    public override IntentKind Kind => IntentKind.ManagerElection;

    public static ManagerElectionIntent Open(Guid communityId, Guid authorMembershipId, Guid targetMembershipId,
        string text, int eligibleVoterCount, bool targetIsConfirmedMember, DateTime now)
    {
        var intent = new ManagerElectionIntent();
        intent.RaiseOpenedAbout(
            UserTargetingIntentEvent.Opened(intent.Kind, communityId, authorMembershipId, targetMembershipId, text,
                eligibleVoterCount, now),
            targetIsConfirmedMember);

        return intent;
    }
}
