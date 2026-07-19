using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Zajednica.BuildingBlocks.Infrastructure.Realtime;

[Authorize]
public sealed class RealtimeHub : Hub
{
    public Task JoinChannel(string channel) => Groups.AddToGroupAsync(Context.ConnectionId, channel);

    public Task LeaveChannel(string channel) => Groups.RemoveFromGroupAsync(Context.ConnectionId, channel);
}
