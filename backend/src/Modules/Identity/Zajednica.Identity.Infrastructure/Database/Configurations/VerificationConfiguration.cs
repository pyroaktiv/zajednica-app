using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Infrastructure.Database.Configurations;

public class VerificationConfiguration : IEntityTypeConfiguration<Verification>
{
    public void Configure(EntityTypeBuilder<Verification> builder)
    {
        builder.ToTable("Verifications");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token).IsRequired();
        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.AccountId);
    }
}
