using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Infrastructure.Database.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Username).IsRequired();
        builder.Property(a => a.Email).IsRequired();
        builder.Property(a => a.PasswordHash).IsRequired();

        builder.HasIndex(a => a.Username).IsUnique();
        builder.HasIndex(a => a.Email).IsUnique();

        // Profile is part of the aggregate: one-to-zero/one, its own table, shadow FK on the child,
        // always loaded with the root, and deleted with it (soft delete severs the navigation).
        builder.HasOne(a => a.Profile)
            .WithOne()
            .HasForeignKey<Profile>("AccountId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Profile).AutoInclude();
    }
}
