using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Infrastructure.Database.Configurations;

public class CertificationChallengeConfiguration : IEntityTypeConfiguration<CertificationChallenge>
{
    public void Configure(EntityTypeBuilder<CertificationChallenge> builder)
    {
        builder.ToTable("CertificationChallenges");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Token).IsRequired();
        builder.HasIndex(c => c.Token).IsUnique();
    }
}
