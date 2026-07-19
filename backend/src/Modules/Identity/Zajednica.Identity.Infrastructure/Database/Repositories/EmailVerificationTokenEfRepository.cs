using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Database.Repositories;

internal sealed class EmailVerificationTokenEfRepository(IdentityDbContext db) : IEmailVerificationTokenRepository
{
    public void Add(EmailVerificationToken token) => db.EmailVerificationTokens.Add(token);

    public void Remove(EmailVerificationToken token) => db.EmailVerificationTokens.Remove(token);

    public Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        db.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
}
