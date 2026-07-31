using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.UseCases;
using Zajednica.Community.Core.UseCases.Internal;
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
        AddApplicationServices(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<ICommunityRepository, CommunityEfRepository>();
        services.AddScoped<IMembershipRepository, MembershipEfRepository>();
        services.AddScoped<ICertificationChallengeRepository, CertificationChallengeEfRepository>();
        services.AddScoped<IDocumentRepository, DocumentEfRepository>();
    }

    private static void AddDomainServices(IServiceCollection services)
    {
        services.AddSingleton<Core.Domain.CertificationService>();
        services.AddSingleton<ManagerElectionService>();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<MembershipAccess>();
        services.AddScoped<MembershipNotifier>();
        services.AddScoped<MuteExpiry>();
        services.AddScoped<ICommunityService, CommunityService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<ICertificationService, Core.UseCases.CertificationService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IInternalMembershipAccessService, MembershipAccessService>();
        services.AddScoped<IInternalMembershipDirectoryService, MembershipDirectoryService>();
        services.AddScoped<IInternalMembershipAudienceService, MembershipAudienceService>();
        services.AddScoped<MembershipWriteService>();
        services.AddScoped<IInternalIntentOutcomeService>(sp => sp.GetRequiredService<MembershipWriteService>());
        services.AddScoped<IInternalStarAwardService>(sp => sp.GetRequiredService<MembershipWriteService>());

        services.AddHostedService<MembershipMuteExpiryWorker>();
    }
}
