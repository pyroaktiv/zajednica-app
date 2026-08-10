using Zajednica.Identity.Core.Infrastructural;
using Zajednica.Identity.Core.Infrastructural.RepositoryInterfaces;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Identity.Infrastructure.Devices;

internal sealed class DeviceTokenEfRepository(IdentityDbContext db) : IDeviceTokenRepository
{
    public void Save(Guid accountId, string token, DateTime now)
    {
        var existing = db.DeviceTokens.FirstOrDefault(t => t.Token == token);
        if (existing is null)
            db.DeviceTokens.Add(new DeviceToken(accountId, token, now));
        else
            existing.ReassignTo(accountId, now);

        db.SaveChanges();
    }

    public void RemoveByToken(string token)
    {
        var existing = db.DeviceTokens.FirstOrDefault(t => t.Token == token);
        if (existing is null)
            return;

        db.DeviceTokens.Remove(existing);
        db.SaveChanges();
    }

    public IReadOnlyList<string> TokensFor(IReadOnlyCollection<Guid> accountIds) =>
        accountIds.Count == 0
            ? []
            : db.DeviceTokens.Where(t => accountIds.Contains(t.AccountId)).Select(t => t.Token).ToList();
}
