namespace Zajednica.BuildingBlocks.Core.Realtime;

public interface IRealtimePusher
{
    void PushToUser(Guid accountId, RealtimeMessage message);
    
    void PushToChannel(string channel, RealtimeMessage message);
}
