using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases;

public sealed class ProfileService(IAccountRepository accounts) : IProfileService
{
    public ProfileDto Get(Guid accountId)
    {
        var account = accounts.GetById(accountId)
            ?? throw new NotFoundException("Account not found.");
        return account.ToProfileDto();
    }

    public ProfileDto Update(Guid accountId, UpdateProfileRequest request)
    {
        var account = accounts.GetById(accountId)
            ?? throw new NotFoundException("Account not found.");

        account.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.ContactEmail, request.ImageUrl);
        accounts.Update(account);

        return account.ToProfileDto();
    }
}
