using Microsoft.Extensions.Logging;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Email;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("[Email:DEV] To={To} Subject={Subject}\n{Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }
}
