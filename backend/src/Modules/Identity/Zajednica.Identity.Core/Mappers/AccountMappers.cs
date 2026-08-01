using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Identity.Api.Internal.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Core.Mappers;

public static class AccountMappers
{
    public static AccountUsernameDto ToUsernameDto(this Account account) =>
        new(account.Id, account.Username);

    public static AccountProfileDto ToAccountProfileDto(this Account account, IFileUrlMapper urls) =>
        new(
            account.Id,
            account.Username,
            account.Profile?.FirstName,
            account.Profile?.LastName,
            account.Profile?.Phone,
            account.Profile?.Email,
            urls.ToUrl(account.Profile?.ImageUrl));
}
