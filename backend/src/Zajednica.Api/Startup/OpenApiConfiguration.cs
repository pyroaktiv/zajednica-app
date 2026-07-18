namespace Zajednica.Api.Startup;

// Exposed at /openapi/v1.json in Development (see Program.cs: app.MapOpenApi()).
public static class OpenApiConfiguration
{
    public static IServiceCollection ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }
}
