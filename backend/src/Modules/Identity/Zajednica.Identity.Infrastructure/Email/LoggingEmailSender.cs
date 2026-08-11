using Microsoft.Extensions.Logging;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Email;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public void Send(string toEmail, string subject, string body)
    {
        logger.LogInformation("[Email:DEV] To={To} Subject={Subject}\n{Body}", toEmail, subject, body);
    }
}
