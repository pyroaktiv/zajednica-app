using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Chat.Core.Domain;

namespace Zajednica.Chat.Infrastructure.Database.Configurations;

public class DirectChatConfiguration : IEntityTypeConfiguration<DirectChat>
{
    public void Configure(EntityTypeBuilder<DirectChat> builder)
    {
        builder.ToTable("DirectChats");
    }
}
