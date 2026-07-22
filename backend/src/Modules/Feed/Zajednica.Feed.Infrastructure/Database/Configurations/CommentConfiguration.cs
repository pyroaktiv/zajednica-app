using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Text).IsRequired();
        builder.HasIndex(c => new { c.PostId, c.ParentCommentId, c.Date, c.Id });

        builder.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
