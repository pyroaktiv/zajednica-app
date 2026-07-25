using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases.Internal;


public sealed class InternalAccountService : IInternalAccountService
{
    private readonly IAccountRepository _accounts;

    public InternalAccountService(IAccountRepository accounts) => _accounts = accounts;

    public string? GetUsername(Guid accountId) =>
        (_accounts.GetById(accountId))?.Username;

    public IReadOnlyList<AccountUsernameDto> GetUsernames(
        IReadOnlyCollection<Guid> accountIds)
    {
        if (accountIds.Count == 0)
            return [];
        var accounts = _accounts.GetManyByIds(accountIds);
        return accounts.Select(a => a.ToUsernameDto()).ToList();
    }

    public AccountProfileDto? GetProfile(Guid accountId)
    {
        var account = _accounts.GetById(accountId);
        return account?.ToAccountProfileDto();
    }

    public IReadOnlyList<AccountProfileDto> GetProfiles(
        IReadOnlyCollection<Guid> accountIds)
    {
        if (accountIds.Count == 0)
            return [];
        var accounts = _accounts.GetManyByIds(accountIds);
        return accounts.Select(a => a.ToAccountProfileDto()).ToList();
    }
}
