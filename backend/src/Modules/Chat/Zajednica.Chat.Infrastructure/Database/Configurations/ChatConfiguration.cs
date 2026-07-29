using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Infrastructure.Database.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<ChatAggregate>
{
    public void Configure(EntityTypeBuilder<ChatAggregate> builder)
    {
        builder.ToTable("Chats");
        builder.UseTptMappingStrategy();
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => new { c.CommunityId, c.LastActivityAt });

        builder.HasMany(c => c.Participants)
            .WithOne()
            .HasForeignKey("ChatId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
