namespace Zajednica.BuildingBlocks.Core.Notifications;

public record NotificationRequest(
    IReadOnlyCollection<Guid> RecipientAccountIds, string Title, string Body, NotificationPriority Priority)
{
    public NotificationRequest(Guid recipientAccountId, string title, string body, NotificationPriority priority)
        : this([recipientAccountId], title, body, priority) { }
}
