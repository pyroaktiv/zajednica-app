using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases;


public sealed class ProfileService : IProfileService
{
    private readonly IAccountRepository _accounts;

    public ProfileService(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public ProfileDto Get(Guid accountId)
    {
        var account = _accounts.GetById(accountId)
            ?? throw new NotFoundException("Account not found.");
        return account.ToProfileDto();
    }

    public ProfileDto Update(Guid accountId, UpdateProfileRequest request)
    {
        var account = _accounts.GetById(accountId)
            ?? throw new NotFoundException("Account not found.");

        account.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.ContactEmail, request.ImageUrl);
        _accounts.Update(account);

        return account.ToProfileDto();
    }
}
