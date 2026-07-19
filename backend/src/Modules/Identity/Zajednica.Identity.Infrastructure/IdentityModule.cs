using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.BuildingBlocks.Infrastructure.DomainEvents;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.UseCases;
using Zajednica.Identity.Infrastructure.Authentication;
using Zajednica.Identity.Infrastructure.Database;
using Zajednica.Identity.Infrastructure.Database.Repositories;
using Zajednica.Identity.Infrastructure.Email;

namespace Zajednica.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        services.AddDbContext<IdentityDbContext>((sp, o) =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
             .UseDomainEvents(sp));

        AddPersistence(services, configuration);
        AddAuthentication(services);
        AddEmail(services, configuration);

        return services;
    }
    
    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAccountRepository, AccountEfRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenEfRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenEfRepository>();
        services.AddScoped<IIdentityUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());
    }

    private static void AddAuthentication(IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
    }

    private static void AddEmail(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>($"{SmtpOptions.SectionName}:Enabled"))
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        else
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
    }
}
