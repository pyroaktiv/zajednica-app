using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Actions;
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

        foreach (var intent in intents.GetDue(now))
            CloseIfDue(intent, now);
    }

    private void Execute(Intent intent, DateTime now)
    {
        switch (intent.Action)
        {
            case UserTargetingAction { Kind: UserActionKind.Ban } ban:
                outcome.Ban(ban.TargetMembershipId, intent.Id);
                SupersedeOthersAbout(intent, ban.TargetMembershipId, now);
                break;

            case UserTargetingAction { Kind: UserActionKind.ManagerElection } election:
                outcome.ElectManager(election.TargetMembershipId);
                break;
        }
    }

    private void SupersedeOthersAbout(Intent ban, Guid targetMembershipId, DateTime now)
    {
        var open = intents.GetOpenByTarget(ban.CommunityId, targetMembershipId);

        foreach (var other in open.Where(i => i.Id != ban.Id))
        {
            other.Supersede(now);
            intents.Update(other);
            notifier.Changed(other);
        }
    }
}
