using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Email;

/// <summary>
/// Sends mail over SMTP using the built-in <see cref="SmtpClient"/>. Configured entirely from
/// <see cref="SmtpOptions"/> so the same code targets a local relay (dev) or a managed provider
/// (Azure) with no code change.
/// </summary>
public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message, ct);
    }
}
