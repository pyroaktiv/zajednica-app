using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Chat.Core.Domain;

namespace Zajednica.Chat.Infrastructure.Database.Configurations;

public class TemporaryChatConfiguration : IEntityTypeConfiguration<TemporaryChat>
{
    public void Configure(EntityTypeBuilder<TemporaryChat> builder)
    {
        builder.ToTable("TemporaryChats");
    }
}
