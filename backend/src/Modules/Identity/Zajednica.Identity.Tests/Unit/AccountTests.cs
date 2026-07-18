using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Tests.Unit;

public class AccountTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Account NewAccount() => new("pera", "pera@example.com", "hash", Now);

    [Fact]
    public void Constructor_sets_defaults()
    {
        var account = NewAccount();

        account.Username.ShouldBe("pera");
        account.Email.ShouldBe("pera@example.com");
        account.PasswordHash.ShouldBe("hash");
        account.IsEmailVerified.ShouldBeFalse();
        account.Status.ShouldBe(AccountStatus.Active);
        account.DateCreated.ShouldBe(Now);
        account.Profile.ShouldBeNull();
    }

    [Fact]
    public void Constructor_normalizes_username_and_email()
    {
        var account = new Account("  pera  ", "  Pera@Example.COM ", "hash", Now);

        account.Username.ShouldBe("pera");
        account.Email.ShouldBe("pera@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_username(string username) =>
        Should.Throw<EntityValidationException>(() => new Account(username, "pera@example.com", "hash", Now));

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("no@dot")]
    [InlineData("two@@at.com")]
    [InlineData("@nolocal.com")]
    public void Constructor_rejects_invalid_email(string email) =>
        Should.Throw<EntityValidationException>(() => new Account("pera", email, "hash", Now));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_password_hash(string hash) =>
        Should.Throw<EntityValidationException>(() => new Account("pera", "pera@example.com", hash, Now));

    [Fact]
    public void VerifyEmail_sets_flag()
    {
        var account = NewAccount();

        account.VerifyEmail();

        account.IsEmailVerified.ShouldBeTrue();
    }

    [Fact]
    public void VerifyEmail_twice_throws()
    {
        var account = NewAccount();
        account.VerifyEmail();

        Should.Throw<EntityValidationException>(() => account.VerifyEmail());
    }

    [Fact]
    public void UpdateProfile_creates_profile_on_first_use()
    {
        var account = NewAccount();

        account.UpdateProfile("Pera", "Peric", "0641234567", "kontakt@example.com", "img.png");

        account.Profile.ShouldNotBeNull();
        account.Profile!.FirstName.ShouldBe("Pera");
        account.Profile.LastName.ShouldBe("Peric");
        account.Profile.Phone.ShouldBe("0641234567");
        account.Profile.Email.ShouldBe("kontakt@example.com");
        account.Profile.ImageUrl.ShouldBe("img.png");
    }

    [Fact]
    public void UpdateProfile_updates_existing_profile_in_place()
    {
        var account = NewAccount();
        account.UpdateProfile("Pera", "Peric", "0641234567", null, null);
        var profileId = account.Profile!.Id;

        account.UpdateProfile("Petar", "Peric", "0649999999", null, null);

        account.Profile!.Id.ShouldBe(profileId); // same entity, mutated
        account.Profile.FirstName.ShouldBe("Petar");
        account.Profile.Phone.ShouldBe("0649999999");
    }

    [Fact]
    public void Delete_soft_deletes_and_clears_profile()
    {
        var account = NewAccount();
        account.UpdateProfile("Pera", "Peric", "0641234567", null, null);

        account.Delete();

        account.Status.ShouldBe(AccountStatus.Deleted);
        account.Profile.ShouldBeNull();
        account.Username.ShouldBe("pera"); // credentials are kept
    }

    [Fact]
    public void Deleted_account_rejects_further_operations()
    {
        var account = NewAccount();
        account.Delete();

        Should.Throw<EntityValidationException>(() => account.VerifyEmail());
        Should.Throw<EntityValidationException>(() => account.UpdateProfile("Pera", "Peric", "064", null, null));
        Should.Throw<EntityValidationException>(() => account.Delete());
    }
}
