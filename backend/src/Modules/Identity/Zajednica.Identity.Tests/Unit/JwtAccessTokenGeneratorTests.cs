using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Identity.Tests.Unit;

public class JwtAccessTokenGeneratorTests
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-signing-key-at-least-32-bytes-long!!",
            ["Jwt:Issuer"] = "zajednica",
            ["Jwt:Audience"] = "zajednica-app",
            ["Jwt:AccessTokenMinutes"] = "15"
        })
        .Build();

    private readonly JwtAccessTokenGenerator _generator = new(Configuration);

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
    public void Generate_sets_expiry_from_configuration()
    {
        var jwt = _generator.Generate(Guid.NewGuid(), "pera");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        token.ValidTo.ShouldBeInRange(DateTime.UtcNow.AddMinutes(14), DateTime.UtcNow.AddMinutes(16));
    }
}
