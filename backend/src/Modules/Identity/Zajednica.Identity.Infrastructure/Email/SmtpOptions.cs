namespace Zajednica.Identity.Infrastructure.Email;

/// <summary>
/// Bound from the "Smtp" configuration section. Externalized for Azure via env vars
/// (Smtp__Host, Smtp__Username, ...). When Enabled is false (local dev default) a logging sender
/// stands in and no real mail is sent.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public bool Enabled { get; set; }
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromAddress { get; set; } = "no-reply@zajednica.app";
    public string FromName { get; set; } = "zajednica.app";
    public bool UseSsl { get; set; } = true;
}
