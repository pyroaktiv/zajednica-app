using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Chat.Core.Domain;

namespace Zajednica.Chat.Infrastructure.Database.Configurations;

public class VoiceMessageConfiguration : IEntityTypeConfiguration<VoiceMessage>
{
    public void Configure(EntityTypeBuilder<VoiceMessage> builder)
    {
        builder.ToTable("VoiceMessages");
        builder.Property(m => m.AudioUrl).IsRequired();
    }
}
