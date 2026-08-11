namespace Zajednica.Identity.Core.Domain.RepositoryInterfaces;

public interface IAccountRepository
{
    void Add(Account account);
    void Update(Account account);

    Account? GetById(Guid id);
    Account? GetByUsernameOrEmail(string usernameOrEmail);
    IReadOnlyList<Account> GetManyByIds(IReadOnlyCollection<Guid> ids);

    bool ExistsByUsername(string username);
    bool ExistsByEmail(string email);
}
