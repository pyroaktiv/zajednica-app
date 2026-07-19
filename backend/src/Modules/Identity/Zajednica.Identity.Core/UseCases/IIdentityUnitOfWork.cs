namespace Zajednica.Identity.Core.UseCases;

/// <summary>
/// Module-scoped unit of work. All Identity repositories share the same tracking context, so an
/// application service mutates several aggregates and commits them in ONE transaction here — which
/// is also the single point where the domain-event dispatch (post-commit interceptor) fires.
/// Module-specific interface (not a shared one) so DI never confuses it with another module's.
/// </summary>
public interface IIdentityUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
