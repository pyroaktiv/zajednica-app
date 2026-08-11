using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Zajednica.BuildingBlocks.Infrastructure.Realtime;

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

                // WebSocket upgrade can't carry an Authorization header, so SignalR clients pass the
                // token as ?access_token=... on the hub URL. Lift it into the pipeline only there.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(token) && path.StartsWithSegments(RealtimeInstaller.HubPath))
                            context.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services.AddAuthorization();
    }
}
