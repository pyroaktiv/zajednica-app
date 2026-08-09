using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentNotifier(
    MemberDirectory memberDirectory,
    INotificationSender notificationSender,
    IRealtimePusher realtimePusher)
{
    public void Opened(Intent intent)
    {
        NotifyTarget(intent, "Pokrenuta namera", "U zajednici je pokrenuta namera koja se odnosi na vas.",
            NotificationPriority.Default);
        Changed(intent);
    }

    public void Closed(Intent intent, IntentStatus status)
    {
        NotifyTarget(intent, "Namera je zaključena",
            $"Namera koja se odnosi na vas je zaključena sa ishodom {status}.",
            status == IntentStatus.Accepted ? NotificationPriority.High : NotificationPriority.Default);
        Changed(intent);
    }

    public void Changed(Intent intent)
    {
        realtimePusher.PushToChannel(Channels.Intent(intent.Id), new RealtimeMessage("intent.updated", new
        {
            id = intent.Id,
            votesFor = intent.VotesFor,
            votesAgainst = intent.VotesAgainst,
            status = intent.Status.ToString()
        }));
    }

    private void NotifyTarget(Intent intent, string title, string body, NotificationPriority priority)
    {
        if (intent.Initiative is not UserTargetingInitiative targeting)
            return;

        if (memberDirectory.AccountId(targeting.TargetMembershipId) is not { } targetAccountId)
            return;

        notificationSender.Send(new NotificationRequest(targetAccountId, title, body, priority));
    }
}
