namespace Zajednica.BuildingBlocks.Core.Notifications;

public interface INotificationSender
{
    void Send(NotificationRequest request);
}