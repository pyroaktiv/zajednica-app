using Microsoft.AspNetCore.SignalR;
using Zajednica.BuildingBlocks.Core.Realtime;

namespace Zajednica.BuildingBlocks.Infrastructure.Realtime;

public sealed class SignalRRealtimePusher(IHubContext<RealtimeHub> hub) : IRealtimePusher
{
    public Task PushToUserAsync(Guid accountId, RealtimeMessage message, CancellationToken ct = default)
        => hub.Clients.User(accountId.ToString()).SendAsync(message.Event, message.Payload, ct);

    public Task PushToChannelAsync(string channel, RealtimeMessage message, CancellationToken ct = default)
        => hub.Clients.Group(channel).SendAsync(message.Event, message.Payload, ct);
}
