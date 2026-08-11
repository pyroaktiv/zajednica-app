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
    }
}
