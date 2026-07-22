using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Infrastructure.Database.EventStore;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("IntentEvents");
        builder.HasKey(e => new { e.StreamId, e.Sequence });

        builder.Property(e => e.EventType).IsRequired();
        builder.Property(e => e.Payload).HasColumnType("json").IsRequired();
    }
}
