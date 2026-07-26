using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases;

public sealed class IntentClosing(
    IIntentRepository intents,
    IIntentQueryStore intentQueries,
    IInternalMembershipService memberships,
    IntentNotifier notifier)
{
    public bool CloseIfDue(Intent intent, DateTime now)
    {
        if (!intent.ShouldClose(now))
            return false;

        Close(intent, now);

        return true;
    }

    public bool CloseIfDue(IntentView view, DateTime now)
    {
        if (view.Status != IntentStatus.Open || view.Deadline > now)
            return false;

        var intent = intents.Get(view.Id);

        return intent is not null && CloseIfDue(intent, now);
    }

    public void CloseDue(Guid communityId)
    {
        var now = DateTime.UtcNow;

        foreach (var view in intentQueries.GetDueViews(communityId, now))
            CloseIfDue(view, now);
    }

    private void Close(Intent intent, DateTime now)
    {
        var status = intent.Close(now);
        intents.Update(intent);

        if (status == IntentStatus.Accepted)
            Execute(intent);

        notifier.Closed(intent, status);
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
        var open = intentQueries.GetOpenViewsByTarget(ban.CommunityId, targetMembershipId);

        foreach (var view in open.Where(v => v.Id != ban.Id))
        {
            var other = intents.Get(view.Id);
            if (other is null)
                continue;

            other.Supersede(now);
            intents.Update(other);
            notifier.Changed(other);
        }
    }
}
