namespace Zajednica.Identity.Core.Infrastructural.RepositoryInterfaces;

public interface IDeviceTokenRepository
{
    void Save(Guid accountId, string token, DateTime now);
    void RemoveByToken(string token);
    IReadOnlyList<string> TokensFor(IReadOnlyCollection<Guid> accountIds);
}
