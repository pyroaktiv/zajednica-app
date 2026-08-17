using Zajednica.Identity.Api.Internal;
using Zajednica.Identity.Api.Internal.Dto;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases.Internal;

public sealed class InternalProfileService(IAccountRepository accountRepository) : IInternalProfileService
{
    public string? GetUsername(Guid accountId) =>
        (accountRepository.GetById(accountId))?.Username;

    public IReadOnlyList<InternalUsernameDto> GetUsernames(
        IReadOnlyCollection<Guid> accountIds)
    {
        if (accountIds.Count == 0)
            return [];
        var found = accountRepository.GetManyByIds(accountIds);
        return found.Select(a => a.ToUsernameDto()).ToList();
    }

    public InternalProfileDto? GetProfile(Guid accountId)
    {
        var account = accountRepository.GetById(accountId);
        return account?.ToAccountProfileDto();
    }

    public IReadOnlyList<InternalProfileDto> GetProfiles(
        IReadOnlyCollection<Guid> accountIds)
    {
        if (accountIds.Count == 0)
            return [];
        var found = accountRepository.GetManyByIds(accountIds);
        return found.Select(a => a.ToAccountProfileDto()).ToList();
    }
}
