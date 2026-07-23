using Zajednica.Community.Core.Domain;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Infrastructure.Database.Repositories;

internal sealed class CertificateEfRepository(CommunityDbContext db) : ICertificateRepository
{
    public void Add(Certificate certificate)
    {
        db.Certificates.Add(certificate);
        db.SaveChanges();
    }
}
