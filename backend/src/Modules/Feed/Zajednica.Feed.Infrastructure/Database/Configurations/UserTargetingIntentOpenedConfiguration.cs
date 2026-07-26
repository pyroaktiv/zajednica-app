using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class UserTargetingIntentOpenedConfiguration : IEntityTypeConfiguration<UserTargetingIntentOpened>
{
    public void Configure(EntityTypeBuilder<UserTargetingIntentOpened> builder)
    {
        builder.Property(e => e.Kind).HasConversion<string>();
    }
}
