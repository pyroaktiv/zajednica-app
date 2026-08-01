using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Core.Mappers;

public static class ProfileMappers
{
    public static ProfileDto ToProfileDto(this Account account, IFileUrlMapper urls) =>
        new(
            account.Username,
            account.Profile?.FirstName,
            account.Profile?.LastName,
            account.Profile?.Phone,
            account.Profile?.Email,
            urls.ToUrl(account.Profile?.ImageUrl));
}
