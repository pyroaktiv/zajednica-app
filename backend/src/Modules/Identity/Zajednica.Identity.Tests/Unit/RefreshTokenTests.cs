using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Tests.Unit;

public class RefreshTokenTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_sets_fields()
    {
        var accountId = Guid.NewGuid();
        var expiresAt = Now.AddDays(30);

        var token = new RefreshToken(accountId, "rt-abc", expiresAt);

        token.AccountId.ShouldBe(accountId);
        token.Token.ShouldBe("rt-abc");
        token.ExpiresAt.ShouldBe(expiresAt);
    }

    [Fact]
    public void Constructor_rejects_empty_account_id() =>
        Should.Throw<EntityValidationException>(() => new RefreshToken(Guid.Empty, "rt-abc", Now));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_token(string tokenValue) =>
        Should.Throw<EntityValidationException>(() => new RefreshToken(Guid.NewGuid(), tokenValue, Now));

    [Fact]
    public void IsValid_true_before_expiry() =>
        new RefreshToken(Guid.NewGuid(), "rt-abc", Now.AddDays(1)).IsValid(Now).ShouldBeTrue();

    [Fact]
    public void IsValid_false_at_or_after_expiry()
    {
        var token = new RefreshToken(Guid.NewGuid(), "rt-abc", Now);

        token.IsValid(Now).ShouldBeFalse();
        token.IsValid(Now.AddSeconds(1)).ShouldBeFalse();
    }
}
