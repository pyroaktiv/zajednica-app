using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Shouldly;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Identity.Tests.Unit;

public class JwtAccessTokenGeneratorTests
{
    private static readonly JwtOptions Options = new()
    {
        Key = "test-signing-key-at-least-32-bytes-long!!",
        Issuer = "zajednica",
        Audience = "zajednica-app",
        AccessTokenMinutes = 15
    };

    private readonly JwtAccessTokenGenerator _generator = new(Microsoft.Extensions.Options.Options.Create(Options));

    [Fact]
    public void Generate_embeds_account_identity_and_no_roles()
    {
        var accountId = Guid.NewGuid();

        var jwt = _generator.Generate(accountId, "pera");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        token.Issuer.ShouldBe("zajednica");
        token.Audiences.ShouldContain("zajednica-app");
        token.Subject.ShouldBe(accountId.ToString());
        token.Claims.First(c => c.Type == "username").Value.ShouldBe("pera");
        token.Claims.ShouldNotContain(c => c.Type == "role" || c.Type.EndsWith("/role"));
    }

    [Fact]
    public void Generate_sets_expiry_from_options()
    {
        var jwt = _generator.Generate(Guid.NewGuid(), "pera");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        token.ValidTo.ShouldBeInRange(DateTime.UtcNow.AddMinutes(14), DateTime.UtcNow.AddMinutes(16));
    }
}
