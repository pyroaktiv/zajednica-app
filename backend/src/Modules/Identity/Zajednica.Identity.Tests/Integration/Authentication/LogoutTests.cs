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
    public async Task Revokes_the_refresh_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = await RegisterVerifiedAsync(scope);
        var issued = await LoginAsync(scope, email);

        await Controller(scope).Logout(new LogoutRequest(issued.RefreshToken), default);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.RefreshTokens.Any(t => t.Token == issued.RefreshToken).ShouldBeFalse();
    }

    [Fact]
    public async Task Is_idempotent_for_an_unknown_token()
    {
        using var scope = Factory.Services.CreateScope();

        var result = await Controller(scope).Logout(new LogoutRequest("no-such-token"), default);

        result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task A_revoked_token_can_no_longer_be_refreshed()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = await RegisterVerifiedAsync(scope);
        var issued = await LoginAsync(scope, email);
        await Controller(scope).Logout(new LogoutRequest(issued.RefreshToken), default);

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequest(issued.RefreshToken), default));
    }
}
