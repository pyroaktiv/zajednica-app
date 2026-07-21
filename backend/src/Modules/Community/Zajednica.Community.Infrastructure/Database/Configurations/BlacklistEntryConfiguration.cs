using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Infrastructure.Database.Configurations;

public class BlacklistEntryConfiguration : IEntityTypeConfiguration<BlacklistEntry>
{
    public void Configure(EntityTypeBuilder<BlacklistEntry> builder)
    {
        builder.ToTable("BlacklistEntries");
        builder.HasKey(b => b.Id);

        builder.HasIndex(b => new { b.AccountId, b.CommunityId }).IsUnique();
    }
}
