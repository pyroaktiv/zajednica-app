namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

/// <summary>
/// Collection-like repository for the Account aggregate: it tracks changes but does not persist —
/// the unit of work commits (see <see cref="UseCases.IIdentityUnitOfWork"/>). Loading an account
/// brings its Profile along (it is part of the aggregate).
/// </summary>
public interface IAccountRepository
{
    void Add(Account account);

    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetManyByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}
