namespace Zajednica.Identity.Core.UseCases;

public interface IEmailSender
{
    void Send(string toEmail, string subject, string body);
}
