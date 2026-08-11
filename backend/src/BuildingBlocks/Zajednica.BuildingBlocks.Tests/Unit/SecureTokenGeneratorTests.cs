using Shouldly;
using Zajednica.BuildingBlocks.Infrastructure.Security;

namespace Zajednica.BuildingBlocks.Tests.Unit;

public class SecureTokenGeneratorTests
{
    private readonly SecureTokenGenerator _generator = new();

    [Fact]
    public void Generate_returns_nonempty_url_safe_token()
    {
        var token = _generator.Generate();

        token.ShouldNotBeNullOrWhiteSpace();
        token.ShouldNotContain("+");
        token.ShouldNotContain("/");
        token.ShouldNotContain("=");
    }

    [Fact]
    public void Generate_is_unique_per_call() =>
        _generator.Generate().ShouldNotBe(_generator.Generate());

    [Fact]
    public void GenerateShort_returns_a_readable_code_unique_per_call()
    {
        var token = _generator.GenerateShort();

        token.Length.ShouldBe(8);
        token.ShouldAllBe(c => char.IsAsciiLetterOrDigit(c));
        token.ShouldNotBe(_generator.GenerateShort());
    }
}
