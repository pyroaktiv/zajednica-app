using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.UseTptMappingStrategy();
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Text).IsRequired();
        builder.HasIndex(p => new { p.CommunityId, p.DateCreated });

        builder.OwnsMany(p => p.Images, image =>
        {
            image.ToTable("Images");
            image.WithOwner().HasForeignKey("PostId");
            image.HasKey(i => i.Id);
            image.Property(i => i.Id).ValueGeneratedNever();
            image.Property(i => i.Url).IsRequired();
        });

        builder.HasMany(p => p.Comments)
            .WithOne()
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
