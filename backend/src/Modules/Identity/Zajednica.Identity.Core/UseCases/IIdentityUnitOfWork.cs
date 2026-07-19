namespace Zajednica.Identity.Core.UseCases;

public interface IIdentityUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
