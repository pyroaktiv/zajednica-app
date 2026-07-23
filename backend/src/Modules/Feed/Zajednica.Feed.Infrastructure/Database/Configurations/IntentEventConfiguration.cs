using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class IntentEventConfiguration : IEntityTypeConfiguration<IntentEvent>
{
    public void Configure(EntityTypeBuilder<IntentEvent> builder)
    {
        builder.ToTable("IntentEvents");
        builder.UseTptMappingStrategy();
        builder.HasKey(e => new { e.StreamId, e.Sequence });

        builder.Property(e => e.Type).HasConversion<string>().IsRequired();
        builder.Property(e => e.Kind).HasConversion<string>().IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().IsRequired();
        builder.Property(e => e.Text).IsRequired();
    }
}
