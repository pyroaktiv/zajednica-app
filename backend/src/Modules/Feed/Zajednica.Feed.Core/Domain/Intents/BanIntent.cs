using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Core.Domain.Intents;

public sealed class BanIntent : Intent
{
    internal BanIntent() { }

    public override string IntentType => "BAN";

    public static BanIntent Open(Guid communityId, Guid authorMembershipId, Guid targetMembershipId, string text,
        int eligibleVoterCount, bool targetEligible, DateTime now)
    {
        var intent = new BanIntent();
        intent.Raise(Validated(
            new BanIntentOpened(now, communityId, authorMembershipId, targetMembershipId, text?.Trim() ?? string.Empty,
                now.Add(VotingWindow), eligibleVoterCount),
            targetEligible));

        return intent;
    }
}
