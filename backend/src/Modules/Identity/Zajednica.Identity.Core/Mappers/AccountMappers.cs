using Zajednica.Identity.Api.Internal.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Core.Mappers;

public static class AccountMappers
{
    public static InternalUsernameDto ToUsernameDto(this Account account) =>
        new(account.Id, account.Username);

    public static InternalProfileDto ToAccountProfileDto(this Account account) =>
        new(
            account.Id,
            account.Username,
            account.Profile?.FirstName,
            account.Profile?.LastName,
            account.Profile?.Phone,
            account.Profile?.Email,
            account.ImageUrl());
}
