using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class UserTargetingIntentEventConfiguration : IEntityTypeConfiguration<UserTargetingIntentEvent>
{
    public void Configure(EntityTypeBuilder<UserTargetingIntentEvent> builder)
    {
        builder.ToTable("UserTargetingIntentEvents");
    }
}
