using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class IntentViewConfiguration : IEntityTypeConfiguration<IntentView>
{
    public void Configure(EntityTypeBuilder<IntentView> builder)
    {
        builder.ToTable("IntentViews");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Kind).IsRequired();
        builder.Property(v => v.Status).HasConversion<string>().IsRequired();
        builder.Property(v => v.Text).IsRequired();

        builder.HasIndex(v => new { v.CommunityId, v.DateCreated });

        builder.HasIndex(v => v.Deadline)
            .HasFilter($"\"Status\" = '{nameof(IntentStatus.Open)}'");

        builder.HasIndex(v => new { v.CommunityId, v.TargetMembershipId })
            .HasFilter($"\"Status\" = '{nameof(IntentStatus.Open)}'");
    }
}
