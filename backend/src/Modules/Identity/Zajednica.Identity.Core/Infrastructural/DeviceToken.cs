using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Identity.Core.Infrastructural;

public class DeviceToken
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime UpdatedAt { get; private set; }

    private DeviceToken() { }

    public DeviceToken(Guid accountId, string token, DateTime now)
    {
        if (accountId == Guid.Empty)
            throw new EntityValidationException("AccountId is required.");
        if (string.IsNullOrWhiteSpace(token))
            throw new EntityValidationException("Token is required.");

        AccountId = accountId;
        Token = token;
        UpdatedAt = now;
    }

    public void ReassignTo(Guid accountId, DateTime now)
    {
        AccountId = accountId;
        UpdatedAt = now;
    }
}
