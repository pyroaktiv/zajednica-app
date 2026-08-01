using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentClosing(
    IIntentRepository intents,
    IInternalIntentOutcomeService outcome,
    IntentNotifier notifier)
{
    public bool CloseIfDue(Intent intent, DateTime now)
    {
        if (!intent.ShouldClose(now))
            return false;

        var status = intent.Close(now);
        intents.Update(intent);

        if (status == IntentStatus.Accepted)
            Execute(intent, now);

        notifier.Closed(intent, status);

        return true;
    }

    public void CloseDue()
    {
        var now = DateTime.UtcNow;

        foreach (var intentId in intents.GetDueIds(now))
        {
            if (intents.Get(intentId) is { } intent)
                CloseIfDue(intent, now);
        }
    }

    private void Execute(Intent intent, DateTime now)
    {
        switch (intent.Initiative)
        {
            case UserTargetingInitiative { Kind: UserActionKind.Ban } ban:
                outcome.Ban(ban.TargetMembershipId, intent.Id);
                SupersedeOthersAbout(intent, ban.TargetMembershipId, now);
                break;

            case UserTargetingInitiative { Kind: UserActionKind.ManagerElection } election:
                outcome.ElectManager(election.TargetMembershipId);
                break;

            case UserTargetingInitiative { Kind: UserActionKind.Mute } mute:
                outcome.Mute(mute.TargetMembershipId);
                break;
        }
    }

    private void SupersedeOthersAbout(Intent ban, Guid targetMembershipId, DateTime now)
    {
        var open = intents.GetOpenByTarget(ban.Initiative.CommunityId, targetMembershipId);

        foreach (var other in open.Where(i => i.Id != ban.Id))
        {
            other.Supersede(now);
            intents.Update(other);
            notifier.Changed(other);
        }
    }
}
