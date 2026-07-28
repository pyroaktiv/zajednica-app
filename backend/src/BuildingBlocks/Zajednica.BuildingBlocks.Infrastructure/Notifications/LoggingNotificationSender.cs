using Microsoft.Extensions.Logging;
using Zajednica.BuildingBlocks.Core.Notifications;

namespace Zajednica.BuildingBlocks.Infrastructure.Notifications;

public sealed class LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) : INotificationSender
{
    public void Send(NotificationRequest request)
    {
        if (request.RecipientAccountIds.Count == 0)
            return;

        logger.LogInformation("NOTIFY [{Priority}] {Recipients}: {Title}",
            request.Priority, string.Join(", ", request.RecipientAccountIds), request.Title);
    }
}
