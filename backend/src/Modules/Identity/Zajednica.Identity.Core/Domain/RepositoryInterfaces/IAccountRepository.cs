namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IAccountRepository
{
    void Add(Account account);

    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetManyByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}
