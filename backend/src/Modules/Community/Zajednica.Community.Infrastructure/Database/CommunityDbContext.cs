using Microsoft.EntityFrameworkCore;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Infrastructure.Database;

public class CommunityDbContext(DbContextOptions<CommunityDbContext> options) : DbContext(options)
{
    public DbSet<Core.Domain.Community> Communities => Set<Core.Domain.Community>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<CertificationChallenge> CertificationChallenges => Set<CertificationChallenge>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<BlacklistEntry> BlacklistEntries => Set<BlacklistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("community");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
