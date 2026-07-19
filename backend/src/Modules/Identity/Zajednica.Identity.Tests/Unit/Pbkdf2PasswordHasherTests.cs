using Shouldly;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Identity.Tests.Unit;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_true_for_correct_password()
    {
        var hash = _hasher.Hash("Tajna123!");

        _hasher.Verify("Tajna123!", hash).ShouldBeTrue();
    }

    [Fact]
    public void Verify_false_for_wrong_password()
    {
        var hash = _hasher.Hash("Tajna123!");

        _hasher.Verify("pogresna", hash).ShouldBeFalse();
    }

    [Fact]
    public void Hash_is_salted_so_same_password_yields_different_hashes() =>
        _hasher.Hash("Tajna123!").ShouldNotBe(_hasher.Hash("Tajna123!"));

    [Theory]
    [InlineData("")]
    [InlineData("no-dot-separator")]
    [InlineData("only.one")]
    public void Verify_false_for_malformed_hash(string malformed) =>
        _hasher.Verify("Tajna123!", malformed).ShouldBeFalse();
}
