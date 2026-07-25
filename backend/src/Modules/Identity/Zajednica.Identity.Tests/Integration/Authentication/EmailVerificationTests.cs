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
    public void Verifies_the_account_and_consumes_the_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (_, accountId) = Register(scope);
        var db = Db(scope);
        var token = db.EmailVerificationTokens.Single(t => t.AccountId == accountId).Token;

        Controller(scope).VerifyEmail(new VerifyEmailRequest(token));

        db.ChangeTracker.Clear();
        db.Accounts.Single(a => a.Id == accountId).IsEmailVerified.ShouldBeTrue();
        db.EmailVerificationTokens.Any(t => t.AccountId == accountId).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_an_unknown_token()
    {
        using var scope = Factory.Services.CreateScope();

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).VerifyEmail(new VerifyEmailRequest("no-such-token")));
    }

    [Fact]
    public void Rejects_and_consumes_an_expired_token()
    {
        using var scope = Factory.Services.CreateScope();
        var (_, accountId) = Register(scope);
        var db = Db(scope);
        var tokenValue = $"expired-{Guid.NewGuid():N}";
        db.EmailVerificationTokens.RemoveRange(db.EmailVerificationTokens.Where(t => t.AccountId == accountId));
        var expired = new EmailVerificationToken(accountId, tokenValue, DateTime.UtcNow.AddHours(-1));
        db.EmailVerificationTokens.Add(expired);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).VerifyEmail(new VerifyEmailRequest(tokenValue)));

        db.ChangeTracker.Clear();
        db.EmailVerificationTokens.Any(t => t.Token == tokenValue).ShouldBeFalse();
        db.Accounts.Single(a => a.Id == accountId).IsEmailVerified.ShouldBeFalse();
    }

    [Fact]
    public void Rejects_verifying_an_already_verified_account()
    {
        using var scope = Factory.Services.CreateScope();
        var (_, accountId) = RegisterVerified(scope);
        var db = Db(scope);
        var tokenValue = $"second-{Guid.NewGuid():N}";
        var second = new EmailVerificationToken(accountId, tokenValue, DateTime.UtcNow.AddHours(24));
        db.EmailVerificationTokens.Add(second);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        Should.Throw<EntityValidationException>(() =>
            Controller(scope).VerifyEmail(new VerifyEmailRequest(tokenValue)));
    }
}
