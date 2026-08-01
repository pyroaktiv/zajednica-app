using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases;

public sealed class ProfileService(IAccountRepository accounts, IFileUrlMapper urls) : IProfileService
{
    public ProfileDto Get(Guid accountId)
    {
        var account = accounts.GetById(accountId)
            ?? throw new NotFoundException("Account not found.");
        return account.ToProfileDto(urls);
    }

    public ProfileDto Update(Guid accountId, UpdateProfileRequest request)
    {
        var account = accounts.GetById(accountId)
            ?? throw new NotFoundException("Account not found.");

        account.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.ContactEmail,
            urls.ToKey(request.ImageUrl));
        accounts.Update(account);

        return account.ToProfileDto(urls);
    }
}
