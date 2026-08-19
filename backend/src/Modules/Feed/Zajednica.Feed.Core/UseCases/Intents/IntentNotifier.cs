using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.UseCases.Intents;

public sealed class IntentNotifier(IRealtimePusher realtimePusher)
{
    public void Changed(Intent intent)
    {
        realtimePusher.PushToChannel(Channels.Intent(intent.Id), new RealtimeMessage("intent.updated", new
        {
            id = intent.Id,
            votesFor = intent.VotesFor,
            votesAgainst = intent.VotesAgainst,
            status = intent.Status.ToString()
        }));
    }
}
