using Microsoft.AspNetCore.SignalR;
using Zajednica.BuildingBlocks.Core.Realtime;

namespace Zajednica.BuildingBlocks.Infrastructure.Realtime;

public sealed class SignalRRealtimePusher(IHubContext<RealtimeHub> hub) : IRealtimePusher
{
    public void PushToUser(Guid accountId, RealtimeMessage message)
        => hub.Clients.User(accountId.ToString()).SendAsync(message.Event, message.Payload).GetAwaiter().GetResult();

    public void PushToChannel(string channel, RealtimeMessage message)
        => hub.Clients.Group(channel).SendAsync(message.Event, message.Payload).GetAwaiter().GetResult();
}
