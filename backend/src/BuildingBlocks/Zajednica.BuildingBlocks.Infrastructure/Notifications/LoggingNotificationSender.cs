using Microsoft.Extensions.Logging;
using Zajednica.BuildingBlocks.Core.Notifications;

namespace Zajednica.BuildingBlocks.Infrastructure.Notifications;

public sealed class LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) : INotificationSender
{
    public Task SendAsync(NotificationRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("NOTIFY [{Priority}] {Recipient}: {Title}", request.Priority, request.RecipientAccountId, request.Title);
        return Task.CompletedTask;
    }
}