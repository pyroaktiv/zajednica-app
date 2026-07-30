using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class IntentClosedConfiguration : IEntityTypeConfiguration<IntentClosed>
{
    public void Configure(EntityTypeBuilder<IntentClosed> builder)
    {
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.Reason).HasConversion<string>();
    }
}
