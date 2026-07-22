namespace Zajednica.Feed.Core.UseCases;

public static class Channels
{
    public static string Community(Guid communityId) => $"community:{communityId}";

    public static string Intent(Guid intentId) => $"intent:{intentId}";
}
