using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentClosingService(
    IIntentRepository intentRepository,
    IInternalMembershipCommandService internalCommandService,
    IntentNotifier notifier)
{
    public bool CloseIfDue(Intent intent, DateTime now)
    {
        if (!intent.ShouldClose(now))
            return false;

        var status = intent.Close(now);
        intentRepository.Update(intent);

        if (status == IntentStatus.Accepted)
            Execute(intent, now);

        notifier.Closed(intent, status);

        return true;
    }

    public void CloseDue()
    {
        var now = DateTime.UtcNow;

        foreach (var intentId in intentRepository.GetDueIds(now))
        {
            if (intentRepository.LoadFromSource(intentId) is { } intent)
                CloseIfDue(intent, now);
        }
    }

    private void Execute(Intent intent, DateTime now)
    {
        switch (intent.Initiative)
        {
            case UserTargetingInitiative { Kind: UserActionKind.Ban } ban:
                internalCommandService.Ban(ban.TargetMembershipId, intent.Id);
                SupersedeRest(intent, ban.TargetMembershipId, now, kind: null);
                break;

            case UserTargetingInitiative { Kind: UserActionKind.ManagerElection } election:
                internalCommandService.ElectManager(election.TargetMembershipId);
                SupersedeRest(intent, election.TargetMembershipId, now, UserActionKind.ManagerElection);
                break;

            case UserTargetingInitiative { Kind: UserActionKind.Mute } mute:
                internalCommandService.Mute(mute.TargetMembershipId);
                SupersedeRest(intent, mute.TargetMembershipId, now, UserActionKind.Mute);
                break;
        }
    }

    private void SupersedeRest(Intent decidedIntent, Guid targetMembershipId, DateTime now, UserActionKind? kind)
    {
        var open = intentRepository.GetOpenByTarget(decidedIntent.Initiative.CommunityId, targetMembershipId);

        foreach (var other in open.Where(i => i.Id != decidedIntent.Id && IsOfUserActionKind(i, kind)))
        {
            other.Supersede(now);
            intentRepository.Update(other);
            notifier.Changed(other);
        }
    }

    private static bool IsOfUserActionKind(Intent intent, UserActionKind? kind) =>
        kind is null || (intent.Initiative is UserTargetingInitiative targeting && targeting.Kind == kind);
}
