namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IRefreshTokenRepository
{
    void Add(RefreshToken token);
    void Remove(RefreshToken token);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
}
