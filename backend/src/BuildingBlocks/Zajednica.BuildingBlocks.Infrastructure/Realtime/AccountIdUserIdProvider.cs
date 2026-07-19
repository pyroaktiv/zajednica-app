using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Zajednica.BuildingBlocks.Infrastructure.Realtime;

// Ties a connection to its account id (JWT "sub"/NameIdentifier) so Clients.User(accountId) resolves.
// Same claim lookup the controllers use via ClaimsPrincipal.AccountId().
public sealed class AccountIdUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => (connection.User?.FindFirst(ClaimTypes.NameIdentifier)
            ?? connection.User?.FindFirst("sub"))?.Value;
}
