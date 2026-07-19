using Shouldly;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Identity.Tests.Unit;

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
}
