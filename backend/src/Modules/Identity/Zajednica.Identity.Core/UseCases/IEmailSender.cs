namespace Zajednica.Identity.Core.UseCases;

/// <summary>
/// Sends a transactional email (currently the account-activation link). Implemented in
/// Infrastructure over SMTP; a logging fallback stands in for local development.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default);
}
