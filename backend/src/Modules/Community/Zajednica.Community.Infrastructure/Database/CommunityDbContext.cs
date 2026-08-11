using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Infrastructure.Database;

public class CommunityDbContext(DbContextOptions<CommunityDbContext> options) : DbContext(options)
{
    public DbSet<Core.Domain.Community> Communities => Set<Core.Domain.Community>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<CertificationChallenge> CertificationChallenges => Set<CertificationChallenge>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("community");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
