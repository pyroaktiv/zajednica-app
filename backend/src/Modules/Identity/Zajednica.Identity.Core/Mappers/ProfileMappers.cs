using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Core.Mappers;

public static class ProfileMappers
{
    public static ProfileDto ToProfileDto(this Account account) =>
        new(
            account.Username,
            account.Profile?.FirstName,
            account.Profile?.LastName,
            account.Profile?.Phone,
            account.Profile?.Email,
            account.Profile?.ImageUrl);
}
