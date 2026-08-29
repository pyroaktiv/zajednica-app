using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class IntentVoteViewConfiguration : IEntityTypeConfiguration<IntentVoteView>
{
    public void Configure(EntityTypeBuilder<IntentVoteView> builder)
    {
        builder.ToTable("IntentVoteViews");
        builder.HasKey(v => new { v.IntentId, v.VoterMembershipId });

        builder.HasIndex(v => new { v.IntentId, v.OccurredAt });
    }
}
