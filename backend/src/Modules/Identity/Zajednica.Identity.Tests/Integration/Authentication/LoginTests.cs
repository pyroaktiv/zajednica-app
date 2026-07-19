using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Tests.Integration.Authentication;

[Collection("Sequential")]
public class LoginTests : BaseIdentityIntegrationTest
{
    public LoginTests(IdentityTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Issues_a_token_pair_carrying_the_account_identity()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, accountId) = await RegisterVerifiedAsync(scope);

        var tokens = await LoginAsync(scope, email);

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
        jwt.Subject.ShouldBe(accountId.ToString());
        jwt.Claims.Single(c => c.Type == "username").Value.ShouldBe(email);

        // The refresh token was persisted so it can later be rotated.
        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.RefreshTokens.Count(t => t.Token == tokens.RefreshToken && t.AccountId == accountId).ShouldBe(1);
    }

    [Fact]
    public async Task Accepts_login_by_username_as_well_as_email()
    {
        using var scope = Factory.Services.CreateScope();
        var username = $"user-{Guid.NewGuid():N}";
        var email = UniqueEmail();
        // Register an account whose username differs from the email, then log in by username.
        await Controller(scope).Register(new RegisterAccountRequest(username, email, ValidPassword, null, null, null, null), default);
        var db = Db(scope);
        var accountId = db.Accounts.Single(a => a.Email == email).Id;
        var token = db.EmailVerificationTokens.Single(t => t.AccountId == accountId).Token;
        await Controller(scope).VerifyEmail(new VerifyEmailRequest(token), default);

        var tokens = await LoginAsync(scope, username);

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Rejects_login_before_the_email_is_verified()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = await RegisterAsync(scope);

        await Should.ThrowAsync<EntityValidationException>(() => LoginAsync(scope, email));
    }

    [Fact]
    public async Task Rejects_a_wrong_password()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = await RegisterVerifiedAsync(scope);

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Login(new LoginRequest(email, "wrong-password"), default));
    }

    [Fact]
    public async Task Rejects_an_unknown_user()
    {
        using var scope = Factory.Services.CreateScope();

        await Should.ThrowAsync<EntityValidationException>(() => LoginAsync(scope, "nobody@test.local"));
    }

    [Fact]
    public async Task Rejects_login_for_a_deleted_account()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, accountId) = await RegisterVerifiedAsync(scope);
        var db = Db(scope);
        var account = db.Accounts.Single(a => a.Id == accountId);
        account.Delete(); // soft delete
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await Should.ThrowAsync<EntityValidationException>(() => LoginAsync(scope, email));
    }
}
