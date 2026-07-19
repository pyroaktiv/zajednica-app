using Microsoft.Extensions.Logging;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Email;

/// <summary>
/// Local-development stand-in: logs the email instead of sending it (mirrors the notifications
/// module's LoggingNotificationSender). Selected when Smtp:Enabled is false.
/// </summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("[Email:DEV] To={To} Subject={Subject}\n{Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }
}
