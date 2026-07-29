using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Chat.Core.Domain;

namespace Zajednica.Chat.Infrastructure.Database.Configurations;

public class TextMessageConfiguration : IEntityTypeConfiguration<TextMessage>
{
    public void Configure(EntityTypeBuilder<TextMessage> builder)
    {
        builder.ToTable("TextMessages");
        builder.Property(m => m.Text).IsRequired();
    }
}
