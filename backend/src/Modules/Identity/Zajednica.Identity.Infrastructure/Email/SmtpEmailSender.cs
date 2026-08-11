using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Email;

public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    private readonly string? _host = configuration["Smtp:Host"];
    private readonly int _port = configuration.GetValue("Smtp:Port", 587);
    private readonly string? _username = configuration["Smtp:Username"];
    private readonly string? _password = configuration["Smtp:Password"];
    private readonly string _fromAddress = configuration["Smtp:FromAddress"] ?? "no-reply@zajednica.app";
    private readonly string _fromName = configuration["Smtp:FromName"] ?? "zajednica.app";
    private readonly bool _useSsl = configuration.GetValue("Smtp:UseSsl", true);

    public void Send(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_host, _port)
        {
            EnableSsl = _useSsl,
            Credentials = new NetworkCredential(_username, _password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_fromAddress, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        client.Send(message);
    }
}
