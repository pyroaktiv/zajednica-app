using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class ManagerElectionIntent : Intent
{
    internal ManagerElectionIntent() { }

    public override string IntentType => "MANAGER_ELECTION";

    public static ManagerElectionIntent Open(Guid communityId, Guid authorMembershipId, Guid targetMembershipId, string text,
        int eligibleVoterCount, bool targetEligible, DateTime now)
    {
        var intent = new ManagerElectionIntent();
        intent.Raise(Validated(
            new ManagerElectionIntentOpened(now, communityId, authorMembershipId, targetMembershipId, text?.Trim() ?? string.Empty,
                now.Add(VotingWindow), eligibleVoterCount),
            targetEligible));

        return intent;
    }
}
