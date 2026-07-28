namespace Zajednica.BuildingBlocks.Core.Realtime;

// Channel names are a contract between the module that pushes and the module that listens,
// so they live next to IRealtimePusher rather than being spelled out per module.
public static class Channels
{
    public static string Community(Guid communityId) => $"community:{communityId}";

    public static string Intent(Guid intentId) => $"intent:{intentId}";
}
