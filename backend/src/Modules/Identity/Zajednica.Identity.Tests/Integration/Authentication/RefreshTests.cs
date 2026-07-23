using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Tests.Integration.Authentication;

[Collection("Sequential")]
public class RefreshTests : BaseIdentityIntegrationTest
{
    public RefreshTests(IdentityTestFactory factory) : base(factory) { }

    [Fact]
    public void Rotates_the_refresh_token_removing_the_old_and_persisting_the_new()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, accountId) = RegisterVerified(scope);
        var issued = Login(scope, email);

        var result = Controller(scope).Refresh(new RefreshRequest(issued.RefreshToken));
        var refreshed = Value<AuthTokens>(result.Result!);

        refreshed.RefreshToken.ShouldNotBe(issued.RefreshToken);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.RefreshTokens.Any(t => t.Token == issued.RefreshToken).ShouldBeFalse();
        db.RefreshTokens.Count(t => t.Token == refreshed.RefreshToken && t.AccountId == accountId).ShouldBe(1);
    }

    [Fact]
    public void Rejects_an_unknown_refresh_token()
    {
        using var scope = Factory.Services.CreateScope();

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequest("no-such-token")));
    }

    [Fact]
    public void A_rotated_token_cannot_be_reused()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = RegisterVerified(scope);
        var issued = Login(scope, email);
        Controller(scope).Refresh(new RefreshRequest(issued.RefreshToken));

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequest(issued.RefreshToken)));
    }

    [Fact]
    public void Rejects_and_consumes_an_expired_refresh_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (_, accountId) = RegisterVerified(scope);
        var db = Db(scope);
        var tokenValue = $"expired-{Guid.NewGuid():N}";
        var expired = new RefreshToken(accountId, tokenValue, DateTime.UtcNow.AddDays(-1));
        db.RefreshTokens.Add(expired);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequest(tokenValue)));

        db.ChangeTracker.Clear();
        db.RefreshTokens.Any(t => t.Token == tokenValue).ShouldBeFalse();
    }
}
