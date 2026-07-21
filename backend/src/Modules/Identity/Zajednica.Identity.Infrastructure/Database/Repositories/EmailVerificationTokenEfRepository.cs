using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Database.Repositories;

internal sealed class EmailVerificationTokenEfRepository(IdentityDbContext db) : IEmailVerificationTokenRepository
{
    public Task AddAsync(EmailVerificationToken token, CancellationToken ct = default)
    {
        db.EmailVerificationTokens.Add(token);
        return db.SaveChangesAsync(ct);
    }

    public Task RemoveAsync(EmailVerificationToken token, CancellationToken ct = default)
    {
        db.EmailVerificationTokens.Remove(token);
        return db.SaveChangesAsync(ct);
    }

    public Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        db.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
}
