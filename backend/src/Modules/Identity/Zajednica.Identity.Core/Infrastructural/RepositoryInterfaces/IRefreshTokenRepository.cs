namespace Zajednica.Identity.Core.Infrastructural.RepositoryInterfaces;

public interface IRefreshTokenRepository
{
    void Add(RefreshToken token);
    void Remove(RefreshToken token);

    RefreshToken? GetByToken(string token);
}
