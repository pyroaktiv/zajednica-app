using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Identity.Api.Internal;

/// <summary>
/// Internal seam consumed by OTHER modules (they reference Identity.Api) to resolve account facts
/// by id. Synchronous "give me a fact" query, per the module-communication rule. Batch overloads
/// let a caller resolve many ids in one round-trip (e.g. rendering a chat member list) — the point
/// where a Redis cache could later slot in behind this interface without touching callers.
/// </summary>
public interface IAccountDirectory
{
    Task<string?> GetUsernameAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<AccountUsernameDto>> GetUsernamesAsync(IReadOnlyCollection<Guid> accountIds, CancellationToken ct = default);

    Task<AccountProfileDto?> GetProfileAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<AccountProfileDto>> GetProfilesAsync(IReadOnlyCollection<Guid> accountIds, CancellationToken ct = default);
}
