using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class IntentViewConfiguration : IEntityTypeConfiguration<IntentView>
{
    public void Configure(EntityTypeBuilder<IntentView> builder)
    {
        builder.ToTable("IntentViews");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.IntentType).IsRequired();
        builder.Property(v => v.Text).IsRequired();
        builder.Property(v => v.Status).HasConversion<string>().IsRequired();

        builder.HasIndex(v => new { v.CommunityId, v.Status });
        builder.HasIndex(v => new { v.CommunityId, v.TargetMembershipId });
    }
}
