namespace Zajednica.BuildingBlocks.Core.Notifications;

public record NotificationRequest(
    IReadOnlyCollection<Guid> RecipientAccountIds, string Title, string Body, NotificationChannel Channel,
    NotificationTarget? Target = null)
{
    public NotificationRequest(Guid recipientAccountId, string title, string body, NotificationChannel channel,
        NotificationTarget? target = null)
        : this([recipientAccountId], title, body, channel, target) { }
}
