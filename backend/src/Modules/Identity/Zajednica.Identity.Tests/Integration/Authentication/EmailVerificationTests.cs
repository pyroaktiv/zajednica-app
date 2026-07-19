using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Tests.Integration.Authentication;

[Collection("Sequential")]
public class EmailVerificationTests : BaseIdentityIntegrationTest
{
    public EmailVerificationTests(IdentityTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Verifies_the_account_and_consumes_the_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (_, accountId) = await RegisterAsync(scope);
        var db = Db(scope);
        var token = db.EmailVerificationTokens.Single(t => t.AccountId == accountId).Token;

        await Controller(scope).VerifyEmail(new VerifyEmailRequest(token), default);

        db.ChangeTracker.Clear();
        db.Accounts.Single(a => a.Id == accountId).IsEmailVerified.ShouldBeTrue();
        db.EmailVerificationTokens.Any(t => t.AccountId == accountId).ShouldBeFalse();
    }

    [Fact]
    public async Task Rejects_an_unknown_token()
    {
        using var scope = Factory.Services.CreateScope();

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).VerifyEmail(new VerifyEmailRequest("no-such-token"), default));
    }

    [Fact]
    public async Task Rejects_and_consumes_an_expired_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (_, accountId) = await RegisterAsync(scope);
        var db = Db(scope);
        var tokenValue = $"expired-{Guid.NewGuid():N}";
        // Replace the fresh token with one that already expired an hour ago.
        db.EmailVerificationTokens.RemoveRange(db.EmailVerificationTokens.Where(t => t.AccountId == accountId));
        var expired = new EmailVerificationToken(accountId, tokenValue, DateTime.UtcNow.AddHours(-1));
        db.EmailVerificationTokens.Add(expired);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).VerifyEmail(new VerifyEmailRequest(tokenValue), default));

        // Expired tokens are removed on use so a stale link cannot be retried.
        db.ChangeTracker.Clear();
        db.EmailVerificationTokens.Any(t => t.Token == tokenValue).ShouldBeFalse();
        db.Accounts.Single(a => a.Id == accountId).IsEmailVerified.ShouldBeFalse();
    }

    [Fact]
    public async Task Rejects_verifying_an_already_verified_account()
    {
        using var scope = Factory.Services.CreateScope();
        var (_, accountId) = await RegisterVerifiedAsync(scope);
        var db = Db(scope);
        // Plant a second, still-valid token for the (now verified) account.
        var tokenValue = $"second-{Guid.NewGuid():N}";
        var second = new EmailVerificationToken(accountId, tokenValue, DateTime.UtcNow.AddHours(24));
        db.EmailVerificationTokens.Add(second);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await Should.ThrowAsync<EntityValidationException>(() =>
            Controller(scope).VerifyEmail(new VerifyEmailRequest(tokenValue), default));
    }
}
