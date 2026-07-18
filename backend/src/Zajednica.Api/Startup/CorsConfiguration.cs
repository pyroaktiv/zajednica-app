namespace Zajednica.Api.Startup;

public static class CorsConfiguration
{
    // Origins come from configuration (Cors:AllowedOrigins), comma-separated.
    // Dev: appsettings.Development.json (Expo on http://localhost:8081).
    // Prod: App Service configuration / environment variable.
    public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration["Cors:AllowedOrigins"]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()));

        return services;
    }
}
