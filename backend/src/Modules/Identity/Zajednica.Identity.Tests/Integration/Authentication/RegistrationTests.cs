using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Tests.Integration.Authentication;

[Collection("Sequential")]
public class RegistrationTests : BaseIdentityIntegrationTest
{
    public RegistrationTests(IdentityTestFactory factory) : base(factory) { }

    [Fact]
    public void Registers_an_unverified_account_with_a_verification_token()
    {
        using var scope = Factory.Services.CreateScope();
        var email = UniqueEmail();

        Controller(scope).Register(new RegisterAccountRequest(email, email, ValidPassword, null, null, null, null));

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var account = db.Accounts.Single(a => a.Email == email);
        account.IsEmailVerified.ShouldBeFalse();
        account.Profile.ShouldBeNull();
        account.Password.ShouldNotBe(ValidPassword);
        account.Password.ShouldContain(".");
        db.EmailVerificationTokens.Count(t => t.AccountId == account.Id).ShouldBe(1);
    }

    [Fact]
    public void Persists_optional_profile_data_when_supplied()
    {
        using var scope = Factory.Services.CreateScope();
        var email = UniqueEmail();

        Controller(scope).Register(
            new RegisterAccountRequest(email, email, ValidPassword, "Petar", "Petrović", "0601234567", "petar@contact.local"));

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
    public void Rejects_a_password_shorter_than_the_minimum_and_persists_nothing()
    {
        using var scope = Factory.Services.CreateScope();
        var email = UniqueEmail();

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).Register(new RegisterAccountRequest(email, email, "short", null, null, null, null)));

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.Accounts.Any(a => a.Email == email).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_a_duplicate_username()
    {
        using var scope = Factory.Services.CreateScope();
        var username = $"user-{Guid.NewGuid():N}";
        Controller(scope).Register(new RegisterAccountRequest(username, UniqueEmail(), ValidPassword, null, null, null, null));

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).Register(new RegisterAccountRequest(username, UniqueEmail(), ValidPassword, null, null, null, null)));
    }

    [Fact]
    public void Rejects_a_duplicate_email()
    {
        using var scope = Factory.Services.CreateScope();
        var email = UniqueEmail();
        Controller(scope).Register(new RegisterAccountRequest($"user-{Guid.NewGuid():N}", email, ValidPassword, null, null, null, null));

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).Register(new RegisterAccountRequest($"user-{Guid.NewGuid():N}", email, ValidPassword, null, null, null, null)));
    }
}
