using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Infrastructure.Database.Configurations;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.CertificationStatus).HasConversion<string>().IsRequired();
        builder.Property(m => m.State).HasConversion<string>().IsRequired();

        builder.HasIndex(m => new { m.AccountId, m.CommunityId }).IsUnique();
        builder.HasIndex(m => m.CommunityId);

        builder.OwnsMany(m => m.Roles, role =>
        {
            role.ToTable("MembershipRoles");
            role.WithOwner().HasForeignKey("MembershipId");
            role.HasKey(r => r.Id);
            role.Property(r => r.Role).HasConversion<string>().IsRequired();
            role.HasIndex("MembershipId", "Role").IsUnique();
        });
        builder.Navigation(m => m.Roles).AutoInclude();
    }
}
