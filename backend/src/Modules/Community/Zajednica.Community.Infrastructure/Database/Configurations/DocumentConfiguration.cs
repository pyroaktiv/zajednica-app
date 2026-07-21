using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Infrastructure.Database.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).IsRequired();
        builder.Property(d => d.Url).IsRequired();

        builder.HasIndex(d => d.CommunityId);
    }
}
