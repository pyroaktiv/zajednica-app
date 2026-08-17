using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class GeneralTopicPostConfiguration : IEntityTypeConfiguration<GeneralTopicPost>
{
    public void Configure(EntityTypeBuilder<GeneralTopicPost> builder)
    {
        builder.ToTable("GeneralTopicPosts");
        builder.Property(p => p.Kind).HasConversion<string>().IsRequired();

        builder.OwnsOne(p => p.Rating, rating =>
        {
            rating.ToTable("CommunityRatings");
            rating.Ignore(r => r.Id);
            rating.Property(r => r.IntentId).IsRequired();
            rating.Property(r => r.Zone).HasConversion<string>().IsRequired();
        });

        builder.Navigation(p => p.Rating).IsRequired(false);
    }
}
