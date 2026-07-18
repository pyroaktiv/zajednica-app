using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Zajednica.Api.Startup;

public static class AuthConfiguration
{
    public static IServiceCollection ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureAuthentication(services, configuration);
        ConfigureAuthorization(services);
        return services;
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // Dev fallbacks so the app runs out of the box. Override via user-secrets locally
        // and via App Service configuration / Key Vault in production.
        var key = configuration["Jwt:Key"] ?? "dev-only-signing-key-change-me-please-32bytes!!";
        var issuer = configuration["Jwt:Issuer"] ?? "zajednica";
        var audience = configuration["Jwt:Audience"] ?? "zajednica-app";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        // Roles in zajednica.app (unconfirmed / confirmed / issuer / manager) live on Membership
        // and are per-community, so they don't map to flat JWT role policies. Named policies get
        // added here once the Identity module and the authorization model are in place.
        services.AddAuthorization();
    }
}
