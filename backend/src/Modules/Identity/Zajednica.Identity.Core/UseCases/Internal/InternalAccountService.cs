using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases.Internal;

public sealed class InternalAccountService(IAccountRepository accounts, IFileUrlMapper urls) : IInternalAccountService
{
    public string? GetUsername(Guid accountId) =>
        (accounts.GetById(accountId))?.Username;

    public IReadOnlyList<AccountUsernameDto> GetUsernames(
        IReadOnlyCollection<Guid> accountIds)
    {
        if (accountIds.Count == 0)
            return [];
        var found = accounts.GetManyByIds(accountIds);
        return found.Select(a => a.ToUsernameDto()).ToList();
    }

    public AccountProfileDto? GetProfile(Guid accountId)
    {
        var account = accounts.GetById(accountId);
        return account?.ToAccountProfileDto(urls);
    }

    public IReadOnlyList<AccountProfileDto> GetProfiles(
        IReadOnlyCollection<Guid> accountIds)
    {
        if (accountIds.Count == 0)
            return [];
        var found = accounts.GetManyByIds(accountIds);
        return found.Select(a => a.ToAccountProfileDto(urls)).ToList();
    }
}
