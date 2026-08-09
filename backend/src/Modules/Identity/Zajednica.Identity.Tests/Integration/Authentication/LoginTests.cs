using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Tests.Integration.Authentication;

[Collection("Sequential")]
public class LoginTests : BaseIdentityIntegrationTest
{
    public LoginTests(IdentityTestFactory factory) : base(factory) { }

    [Fact]
    public void Issues_a_token_pair_carrying_the_account_identity()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, accountId) = RegisterVerified(scope);

        var tokens = Login(scope, email);

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
        jwt.Subject.ShouldBe(accountId.ToString());
        jwt.Claims.Single(c => c.Type == "username").Value.ShouldBe(email);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.RefreshTokens.Count(t => t.Token == tokens.RefreshToken && t.AccountId == accountId).ShouldBe(1);
    }

    [Fact]
    public void Accepts_login_by_username_as_well_as_email()
    {
        using var scope = Factory.Services.CreateScope();
        var username = $"user-{Guid.NewGuid():N}";
        var email = UniqueEmail();
        Controller(scope).Register(new RegisterAccountRequestDto(username, email, ValidPassword, null, null, null, null));
        var db = Db(scope);
        var accountId = db.Accounts.Single(a => a.Email == email).Id;
        var token = db.Verifications.Single(t => t.AccountId == accountId).Token;
        Controller(scope).VerifyEmail(new VerifyEmailRequestDto(token));

        var tokens = Login(scope, username);

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Rejects_login_before_the_email_is_verified()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = Register(scope);

        Should.Throw<EntityValidationException>(() => Login(scope, email));
    }

    [Fact]
    public void Rejects_a_wrong_password()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = RegisterVerified(scope);

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).Login(new LoginRequestDto(email, "wrong-password")));
    }

    [Fact]
    public void Rejects_an_unknown_user()
    {
        using var scope = Factory.Services.CreateScope();

        Should.Throw<EntityValidationException>(() => Login(scope, "nobody@test.local"));
    }
}
