using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Tests.Integration.Authentication;

[Collection("Sequential")]
public class RegistrationTests : BaseIdentityIntegrationTest
{
    public RegistrationTests(IdentityTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Registers_an_active_unverified_account_with_a_verification_token()
    {
        using var scope = Factory.Services.CreateScope();
        var email = UniqueEmail();

        await Controller(scope).Register(new RegisterAccountRequest(email, email, ValidPassword, null, null, null, null), default);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var account = db.Accounts.Single(a => a.Email == email);
        account.Status.ShouldBe(AccountStatus.Active);
        account.IsEmailVerified.ShouldBeFalse();
        account.Profile.ShouldBeNull();
        // Password is stored as a PBKDF2 "salt.hash", never the plaintext.
        account.PasswordHash.ShouldNotBe(ValidPassword);
        account.PasswordHash.ShouldContain(".");
        db.EmailVerificationTokens.Count(t => t.AccountId == account.Id).ShouldBe(1);
    }

    [Fact]
    public async Task Persists_optional_profile_data_when_supplied()
    {
        using var scope = Factory.Services.CreateScope();
        var email = UniqueEmail();

        await Controller(scope).Register(
            new RegisterAccountRequest(email, email, ValidPassword, "Petar", "Petrović", "0601234567", "petar@contact.local"),
            default);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var account = db.Accounts.Single(a => a.Email == email);
        account.Profile.ShouldNotBeNull();
        account.Profile!.FirstName.ShouldBe("Petar");
        account.Profile.LastName.ShouldBe("Petrović");
        account.Profile.Phone.ShouldBe("0601234567");
        account.Profile.Email.ShouldBe("petar@contact.local");
    }

    [Fact]
    public async Task Rejects_a_password_shorter_than_the_minimum_and_persists_nothing()
    {
        using var scope = Factory.Services.CreateScope();
        var email = UniqueEmail();

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Register(new RegisterAccountRequest(email, email, "short", null, null, null, null), default));

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.Accounts.Any(a => a.Email == email).ShouldBeFalse();
    }

    [Fact]
    public async Task Rejects_a_duplicate_username()
    {
        using var scope = Factory.Services.CreateScope();
        var username = $"user-{Guid.NewGuid():N}";
        await Controller(scope).Register(new RegisterAccountRequest(username, UniqueEmail(), ValidPassword, null, null, null, null), default);

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Register(new RegisterAccountRequest(username, UniqueEmail(), ValidPassword, null, null, null, null), default));
    }

    [Fact]
    public async Task Rejects_a_duplicate_email()
    {
        using var scope = Factory.Services.CreateScope();
        var email = UniqueEmail();
        await Controller(scope).Register(new RegisterAccountRequest($"user-{Guid.NewGuid():N}", email, ValidPassword, null, null, null, null), default);

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Register(new RegisterAccountRequest($"user-{Guid.NewGuid():N}", email, ValidPassword, null, null, null, null), default));
    }
}
