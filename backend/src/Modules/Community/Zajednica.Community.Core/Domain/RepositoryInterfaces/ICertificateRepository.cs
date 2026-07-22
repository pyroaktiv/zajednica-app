namespace Zajednica.Community.Core.Domain.RepositoryInterfaces;

public interface ICertificateRepository
{
    Task AddAsync(Certificate certificate, CancellationToken ct = default);
}
