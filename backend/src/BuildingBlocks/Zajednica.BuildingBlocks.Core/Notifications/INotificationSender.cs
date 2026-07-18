namespace Zajednica.BuildingBlocks.Core.Notifications;

public interface INotificationSender
{
    Task SendAsync(NotificationRequest request, CancellationToken ct = default);
}