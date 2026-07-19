namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

/// <summary>Repository for refresh tokens. Rotation = remove the presented token and add a fresh one.</summary>
public interface IRefreshTokenRepository
{
    void Add(RefreshToken token);
    void Remove(RefreshToken token);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
}
