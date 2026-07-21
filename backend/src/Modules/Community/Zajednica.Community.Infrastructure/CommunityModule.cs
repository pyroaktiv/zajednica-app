using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Infrastructure.Database;
using Zajednica.Community.Infrastructure.Database.Repositories;

namespace Zajednica.Community.Infrastructure;

public static class CommunityModule
{
    public static IServiceCollection AddCommunityModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CommunityDbContext>(o =>
            o.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "community")));

        AddPersistence(services);
        AddDomainServices(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<ICommunityRepository, CommunityEfRepository>();
        services.AddScoped<IMembershipRepository, MembershipEfRepository>();
        services.AddScoped<ICertificateRepository, CertificateEfRepository>();
        services.AddScoped<ICertificationChallengeRepository, CertificationChallengeEfRepository>();
        services.AddScoped<IDocumentRepository, DocumentEfRepository>();
        services.AddScoped<IBlacklistRepository, BlacklistEfRepository>();
    }

    private static void AddDomainServices(IServiceCollection services)
    {
        services.AddSingleton<CertificationService>();
        services.AddSingleton<ManagerElectionService>();
        services.AddSingleton<MembershipBanService>();
    }
}
