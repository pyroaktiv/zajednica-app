namespace Zajednica.BuildingBlocks.Core.Realtime;

public interface IRealtimePusher
{
    Task PushToUserAsync(Guid accountId, RealtimeMessage message, CancellationToken ct = default);
    
    Task PushToChannelAsync(string channel, RealtimeMessage message, CancellationToken ct = default);
}
