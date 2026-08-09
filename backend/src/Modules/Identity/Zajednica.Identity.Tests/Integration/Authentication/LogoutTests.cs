using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Tests.Integration.Authentication;

[Collection("Sequential")]
public class LogoutTests : BaseIdentityIntegrationTest
{
    public LogoutTests(IdentityTestFactory factory) : base(factory) { }

    [Fact]
    public void Revokes_the_refresh_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = RegisterVerified(scope);
        var issued = Login(scope, email);

        Controller(scope).Logout(new LogoutRequestDto(issued.RefreshToken));

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.RefreshTokens.Any(t => t.Token == issued.RefreshToken).ShouldBeFalse();
    }

    [Fact]
    public void Is_idempotent_for_an_unknown_token()
    {
        using var scope = Factory.Services.CreateScope();

        var result = Controller(scope).Logout(new LogoutRequestDto("no-such-token"));

        result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public void A_revoked_token_can_no_longer_be_refreshed()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = RegisterVerified(scope);
        var issued = Login(scope, email);
        Controller(scope).Logout(new LogoutRequestDto(issued.RefreshToken));

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequestDto(issued.RefreshToken)));
    }
}
