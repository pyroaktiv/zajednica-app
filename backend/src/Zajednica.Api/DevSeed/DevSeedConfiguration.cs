namespace Zajednica.Api.DevSeed;

public static class DevSeedConfiguration
{
    public static IServiceCollection AddDevSeed(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment() && configuration.GetValue<bool>("Seed:Enabled"))
            services.AddHostedService<DevDataSeeder>();

        return services;
    }
}
