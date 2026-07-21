using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Identity.Api.Internal;

public interface IInternalAccountService
{
    Task<string?> GetUsernameAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<AccountUsernameDto>> GetUsernamesAsync(IReadOnlyCollection<Guid> accountIds, CancellationToken ct = default);

    Task<AccountProfileDto?> GetProfileAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<AccountProfileDto>> GetProfilesAsync(IReadOnlyCollection<Guid> accountIds, CancellationToken ct = default);
}
