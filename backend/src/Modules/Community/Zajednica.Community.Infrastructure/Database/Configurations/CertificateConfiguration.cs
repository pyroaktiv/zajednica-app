using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Infrastructure.Database.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.CommunityId);
        builder.HasIndex(c => new { c.IssuerMembershipId, c.CandidateMembershipId }).IsUnique();
    }
}
