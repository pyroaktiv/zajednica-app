using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Identity.Core.Domain;

public class Account : AggregateRoot
{
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsEmailVerified { get; private set; }
    public DateTime DateCreated { get; private set; }
    public Profile? Profile { get; private set; }

    private Account() { }

    public Account(string username, string email, string passwordHash, DateTime now)
    {
        Username = NormalizeUsername(username);
        Email = NormalizeEmail(email);
        PasswordHash = RequirePasswordHash(passwordHash);
        IsEmailVerified = false;
        DateCreated = now;
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
            throw new EntityValidationException("Email is already verified.");
        IsEmailVerified = true;
    }

    public void UpdateProfile(string? firstName, string? lastName, string? phone, string? contactEmail)
    {
        if (Profile is null)
            Profile = new Profile(firstName, lastName, phone, contactEmail);
        else
            Profile.Update(firstName, lastName, phone, contactEmail);
    }

    public void SetProfileImage(string imageKey)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
            throw new EntityValidationException("An image key is required.");

        Profile ??= new Profile(null, null, null, null);
        Profile.SetImage(imageKey);
    }

    public void RemoveProfileImage() => Profile?.SetImage(null);

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

    private static string RequirePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new EntityValidationException("Password hash is required.");
        return passwordHash;
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
