namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IRefreshTokenRepository
{
    void Add(RefreshToken token);
    void Remove(RefreshToken token);

    RefreshToken? GetByToken(string token);
}
