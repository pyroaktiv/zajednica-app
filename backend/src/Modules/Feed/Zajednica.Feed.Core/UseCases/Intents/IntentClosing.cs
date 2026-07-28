using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentClosing(
    IIntentRepository intents,
    IInternalMembershipService memberships,
    IntentNotifier notifier)
{
    public bool CloseIfDue(Intent intent, DateTime now)
    {
        if (!intent.ShouldClose(now))
            return false;

        var status = intent.Close(now);
        intents.Update(intent);

        if (status == IntentStatus.Accepted)
            Execute(intent);

        notifier.Closed(intent, status);

        return true;
    }

    public void CloseDue()
    {
        var now = DateTime.UtcNow;

        foreach (var intent in intents.GetDue(now))
            CloseIfDue(intent, now);
    }

    private void Execute(Intent intent)
    {
        switch (intent.Action)
        {
            case UserTargetingAction { Kind: UserActionKind.Ban } ban:
                memberships.Ban(ban.TargetMembershipId, intent.Id);
                SupersedeOthersAbout(intent, ban.TargetMembershipId);
                break;

            case UserTargetingAction { Kind: UserActionKind.ManagerElection } election:
                memberships.ElectManager(election.TargetMembershipId);
                break;
        }
    }

    private void SupersedeOthersAbout(Intent ban, Guid targetMembershipId)
    {
        var now = DateTime.UtcNow;
        var open = intents.GetOpenByTarget(ban.CommunityId, targetMembershipId);

        foreach (var other in open.Where(i => i.Id != ban.Id))
        {
            other.Supersede(now);
            intents.Update(other);
            notifier.Changed(other);
        }
    }
}
