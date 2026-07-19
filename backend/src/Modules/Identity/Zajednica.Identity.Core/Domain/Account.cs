using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Identity.Core.Domain;

public class Account : AggregateRoot
{
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsEmailVerified { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime DateCreated { get; private set; }
    public Profile? Profile { get; private set; }

    // EF
    private Account() { }

    public Account(string username, string email, string passwordHash, DateTime now)
    {
        Username = NormalizeUsername(username);
        Email = NormalizeEmail(email);
        PasswordHash = RequireHash(passwordHash);
        IsEmailVerified = false;
        Status = AccountStatus.Active;
        DateCreated = now;
    }
    
    public void VerifyEmail()
    {
        EnsureActive();
        if (IsEmailVerified)
            throw new EntityValidationException("Email is already verified.");
        IsEmailVerified = true;
    }

    /// <summary>Creates the profile on first use, otherwise updates it in place.</summary>
    public void UpdateProfile(string? firstName, string? lastName, string? phone, string? contactEmail, string? imageUrl)
    {
        EnsureActive();
        if (Profile is null)
            Profile = new Profile(firstName, lastName, phone, contactEmail, imageUrl);
        else
            Profile.Update(firstName, lastName, phone, contactEmail, imageUrl);
    }

    /// <summary>Soft delete: keep the account (Status = Deleted), drop the personal data.</summary>
    public void Delete()
    {
        EnsureActive();
        Status = AccountStatus.Deleted;
        Profile = null;
    }

    private void EnsureActive()
    {
        if (Status == AccountStatus.Deleted)
            throw new EntityValidationException("Account is deleted.");
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

    private static string RequireHash(string passwordHash)
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
