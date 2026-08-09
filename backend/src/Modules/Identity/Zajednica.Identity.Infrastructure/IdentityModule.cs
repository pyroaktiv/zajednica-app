using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Infrastructural.RepositoryInterfaces;
using Zajednica.Identity.Core.UseCases;
using Zajednica.Identity.Core.UseCases.Internal;
using Zajednica.Identity.Infrastructure.Authentication;
using Zajednica.Identity.Infrastructure.Database;
using Zajednica.Identity.Infrastructure.Database.Repositories;
using Zajednica.Identity.Infrastructure.Email;

namespace Zajednica.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddSingleton<IAuthTokenSettings, AuthTokenSettings>();

        services.AddDbContext<IdentityDbContext>(o =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

        AddPersistence(services);
        AddAuthentication(services);
        AddEmail(services, configuration);
        AddApplicationServices(services);

        return services;
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IInternalProfileService, InternalProfileService>();
    }
    
    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountEfRepository>();
        services.AddScoped<IVerificationRepository, VerificationEfRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenEfRepository>();
    }

    private static void AddAuthentication(IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
    }

    private static void AddEmail(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Smtp:Enabled"))
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        else
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
    }
}
