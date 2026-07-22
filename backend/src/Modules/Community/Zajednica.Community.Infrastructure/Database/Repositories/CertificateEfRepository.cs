using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class CertificateEfRepository(CommunityDbContext db) : ICertificateRepository
{
    public Task AddAsync(Certificate certificate, CancellationToken ct = default)
    {
        db.Certificates.Add(certificate);
        return db.SaveChangesAsync(ct);
    }
}
