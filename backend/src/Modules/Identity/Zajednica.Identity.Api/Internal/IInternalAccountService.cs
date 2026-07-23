using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Identity.Api.Internal;

public interface IInternalAccountService
{
    string? GetUsername(Guid accountId);
    IReadOnlyList<AccountUsernameDto> GetUsernames(IReadOnlyCollection<Guid> accountIds);

    AccountProfileDto? GetProfile(Guid accountId);
    IReadOnlyList<AccountProfileDto> GetProfiles(IReadOnlyCollection<Guid> accountIds);
}
