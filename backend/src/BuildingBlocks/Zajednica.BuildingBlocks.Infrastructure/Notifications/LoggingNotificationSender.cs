using Microsoft.Extensions.Logging;
using Zajednica.BuildingBlocks.Core.Notifications;

namespace Zajednica.BuildingBlocks.Infrastructure.Notifications;

public sealed class LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) : INotificationSender
{
    public void Send(NotificationRequest request)
    {
        logger.LogInformation("NOTIFY [{Priority}] {Recipient}: {Title}", request.Priority, request.RecipientAccountId, request.Title);
    }
}