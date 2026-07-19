using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases.Internal;


public sealed class AccountDirectory : IAccountDirectory
{
    private readonly IAccountRepository _accounts;

    public AccountDirectory(IAccountRepository accounts) => _accounts = accounts;

    public async Task<string?> GetUsernameAsync(Guid accountId, CancellationToken ct = default) =>
        (await _accounts.GetByIdAsync(accountId, ct))?.Username;

    public async Task<IReadOnlyList<AccountUsernameDto>> GetUsernamesAsync(
        IReadOnlyCollection<Guid> accountIds, CancellationToken ct = default)
    {
        if (accountIds.Count == 0)
            return [];
        var accounts = await _accounts.GetManyByIdsAsync(accountIds, ct);
        return accounts.Select(a => a.ToUsernameDto()).ToList();
    }

    public async Task<AccountProfileDto?> GetProfileAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await _accounts.GetByIdAsync(accountId, ct);
        return account?.ToProfileDto();
    }

    public async Task<IReadOnlyList<AccountProfileDto>> GetProfilesAsync(
        IReadOnlyCollection<Guid> accountIds, CancellationToken ct = default)
    {
        if (accountIds.Count == 0)
            return [];
        var accounts = await _accounts.GetManyByIdsAsync(accountIds, ct);
        return accounts.Select(a => a.ToProfileDto()).ToList();
    }
}
