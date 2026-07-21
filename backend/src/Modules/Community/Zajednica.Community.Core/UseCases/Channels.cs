namespace Zajednica.Community.Core.UseCases;

public static class Channels
{
    public static string Community(Guid communityId) => $"community:{communityId}";
}
