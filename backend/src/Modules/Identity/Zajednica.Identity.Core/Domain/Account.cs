using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Identity.Core.Domain;

public class Account : AggregateRoot
{
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public bool IsEmailVerified { get; private set; }
    public DateTime DateCreated { get; private set; }
    public Profile? Profile { get; private set; }

    private Account() { }

    public Account(string username, string email, string password, DateTime now)
    {
        Username = NormalizeUsername(username);
        Email = NormalizeEmail(email);
        Password = RequirePassword(password);
        IsEmailVerified = false;
        DateCreated = now;
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
            throw new EntityValidationException("Email is already verified.");
        IsEmailVerified = true;
    }

    public void UpdateProfile(string? firstName, string? lastName, string? phone, string? contactEmail, string? imageUrl)
    {
        if (Profile is null)
            Profile = new Profile(firstName, lastName, phone, contactEmail, imageUrl);
        else
            Profile.Update(firstName, lastName, phone, contactEmail, imageUrl);
    }

    private static string NormalizeUsername(string username)
    {
        var value = username?.Trim();
        if (string.IsNullOrEmpty(value))
            throw new EntityValidationException("Username is required.");
        return value;
    }

    private static string NormalizeEmail(string email)
    {
        var value = email?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(value) || !IsValidEmail(value))
            throw new EntityValidationException("A valid email is required.");
        return value;
    }

    private static string RequirePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new EntityValidationException("Password hash is required.");
        return password;
    }

    private static bool IsValidEmail(string email)
    {
        var at = email.IndexOf('@');
        return at > 0
               && at < email.Length - 1
               && email.IndexOf('@', at + 1) < 0
               && email[(at + 1)..].Contains('.');
    }
}
