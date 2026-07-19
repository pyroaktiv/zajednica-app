using Zajednica.Identity.Api.Internal.Dto;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Core.Mappers;

/// <summary>
/// Hand-written projections from the Account aggregate to the Internal DTOs other modules consume.
/// Only data Identity owns is exposed; profile fields are read through the (nullable) Profile, so a
/// profile-less account maps to nulls. The two projections are flat — a mapping library buys nothing
/// here and would only add a dependency, so we keep the seam explicit and dependency-free.
/// </summary>
public static class AccountMappers
{
    public static AccountUsernameDto ToUsernameDto(this Account account) =>
        new(account.Id, account.Username);

    public static AccountProfileDto ToProfileDto(this Account account) =>
        new(
            account.Id,
            account.Username,
            account.Profile?.FirstName,
            account.Profile?.LastName,
            account.Profile?.Phone,
            account.Profile?.Email,
            account.Profile?.ImageUrl);
}
