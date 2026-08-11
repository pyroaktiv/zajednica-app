using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Database.Repositories;

internal sealed class AccountEfRepository(IdentityDbContext db) : IAccountRepository
{
    public void Add(Account account)
    {
        db.Accounts.Add(account);
        db.SaveChanges();
    }

    public void Update(Account account) => db.SaveChanges();

    public Account? GetById(Guid id) =>
        db.Accounts.FirstOrDefault(a => a.Id == id);

    public Account? GetByUsernameOrEmail(string usernameOrEmail)
    {
        var value = usernameOrEmail.Trim();
        var email = value.ToLowerInvariant();
        return db.Accounts.FirstOrDefault(a => a.Username == value || a.Email == email);
    }

    public IReadOnlyList<Account> GetManyByIds(IReadOnlyCollection<Guid> ids) =>
        db.Accounts.Where(a => ids.Contains(a.Id)).ToList();

    public bool ExistsByUsername(string username) =>
        db.Accounts.Any(a => a.Username == username.Trim());

    public bool ExistsByEmail(string email) =>
        db.Accounts.Any(a => a.Email == email.Trim().ToLower());
}
