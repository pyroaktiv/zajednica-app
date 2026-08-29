using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Infrastructure.Database.Configurations;

public class IntentEventConfiguration : IEntityTypeConfiguration<IntentEvent>
{
    public void Configure(EntityTypeBuilder<IntentEvent> builder)
    {
        builder.ToTable("IntentEvents");
        builder.HasKey(e => new { e.StreamId, e.Sequence });

        builder.HasDiscriminator<string>("Type")
            .HasValue<BanIntentOpened>(nameof(BanIntentOpened))
            .HasValue<MuteIntentOpened>(nameof(MuteIntentOpened))
            .HasValue<ManagerElectionIntentOpened>(nameof(ManagerElectionIntentOpened))
            .HasValue<PostRatingIntentOpened>(nameof(PostRatingIntentOpened))
            .HasValue<VoteCast>(nameof(VoteCast))
            .HasValue<IntentClosed>(nameof(IntentClosed));
    }
}
