namespace Zajednica.BuildingBlocks.Core.Notifications;

public record NotificationRequest(Guid RecipientAccountId, string Title, string Body, NotificationPriority Priority);