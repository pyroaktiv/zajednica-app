using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Chat.Core.Domain;

namespace Zajednica.Chat.Infrastructure.Database.Configurations;

public class HelpRequestChatConfiguration : IEntityTypeConfiguration<HelpRequestChat>
{
    public void Configure(EntityTypeBuilder<HelpRequestChat> builder)
    {
        builder.ToTable("HelpRequestChats");

        builder.Property(c => c.Status).HasConversion<string>().IsRequired();
        builder.HasIndex(c => c.HelpRequestId);
    }
}
