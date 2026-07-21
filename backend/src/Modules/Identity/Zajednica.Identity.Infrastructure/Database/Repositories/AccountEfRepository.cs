using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Database.Repositories;

internal sealed class AccountEfRepository(IdentityDbContext db) : IAccountRepository
{
    public Task AddAsync(Account account, CancellationToken ct = default)
    {
        db.Accounts.Add(account);
        return db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Account account, CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Account?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
    {
        var value = usernameOrEmail.Trim();
        var email = value.ToLowerInvariant(); // email is stored normalized to lowercase
        return db.Accounts.FirstOrDefaultAsync(a => a.Username == value || a.Email == email, ct);
    }

    public async Task<IReadOnlyList<Account>> GetManyByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.Accounts.Where(a => ids.Contains(a.Id)).ToListAsync(ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Accounts.AnyAsync(a => a.Username == username.Trim(), ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        db.Accounts.AnyAsync(a => a.Email == email.Trim().ToLower(), ct);
}
