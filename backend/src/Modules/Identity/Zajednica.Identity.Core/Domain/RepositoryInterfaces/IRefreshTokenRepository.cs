namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task RemoveAsync(RefreshToken token, CancellationToken ct = default);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
}
