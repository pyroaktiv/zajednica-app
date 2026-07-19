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
    public async Task Rotates_the_refresh_token_removing_the_old_and_persisting_the_new()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, accountId) = await RegisterVerifiedAsync(scope);
        var issued = await LoginAsync(scope, email);

        var result = await Controller(scope).Refresh(new RefreshRequest(issued.RefreshToken), default);
        var refreshed = Value<AuthTokens>(result.Result!);

        refreshed.RefreshToken.ShouldNotBe(issued.RefreshToken);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.RefreshTokens.Any(t => t.Token == issued.RefreshToken).ShouldBeFalse();
        db.RefreshTokens.Count(t => t.Token == refreshed.RefreshToken && t.AccountId == accountId).ShouldBe(1);
    }

    [Fact]
    public async Task Rejects_an_unknown_refresh_token()
    {
        using var scope = Factory.Services.CreateScope();

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequest("no-such-token"), default));
    }

    [Fact]
    public async Task A_rotated_token_cannot_be_reused()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, _) = await RegisterVerifiedAsync(scope);
        var issued = await LoginAsync(scope, email);
        await Controller(scope).Refresh(new RefreshRequest(issued.RefreshToken), default);

        // Presenting the already-rotated token again must fail (single-use).
        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequest(issued.RefreshToken), default));
    }

    [Fact]
    public async Task Rejects_and_consumes_an_expired_refresh_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (_, accountId) = await RegisterVerifiedAsync(scope);
        var db = Db(scope);
        var tokenValue = $"expired-{Guid.NewGuid():N}";
        var expired = new RefreshToken(accountId, tokenValue, DateTime.UtcNow.AddDays(-1));
        db.RefreshTokens.Add(expired);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequest(tokenValue), default));

        db.ChangeTracker.Clear();
        db.RefreshTokens.Any(t => t.Token == tokenValue).ShouldBeFalse();
    }

    [Fact]
    public async Task Rejects_refresh_for_a_deleted_account_and_consumes_the_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (email, accountId) = await RegisterVerifiedAsync(scope);
        var issued = await LoginAsync(scope, email);
        var db = Db(scope);
        db.Accounts.Single(a => a.Id == accountId).Delete();
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).Refresh(new RefreshRequest(issued.RefreshToken), default));

        db.ChangeTracker.Clear();
        db.RefreshTokens.Any(t => t.Token == issued.RefreshToken).ShouldBeFalse();
    }
}
