using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Authentication;

public sealed class JwtAccessTokenGenerator(IConfiguration configuration) : IAccessTokenGenerator
{
    private readonly string _key = configuration["Jwt:Key"] ?? "";
    private readonly string? _issuer = configuration["Jwt:Issuer"];
    private readonly string? _audience = configuration["Jwt:Audience"];
    private readonly int _accessTokenMinutes = configuration.GetValue("Jwt:AccessTokenMinutes", 15);

    public string Generate(Guid accountId, string username)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new("username", username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
