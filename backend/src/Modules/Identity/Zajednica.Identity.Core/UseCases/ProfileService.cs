using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases;

public sealed class ProfileService(IAccountRepository accountRepository) : IProfileService
{
    public ProfileDto Get(Guid accountId) => Require(accountId).ToProfileDto();

    public ProfileDto Update(Guid accountId, UpdateProfileRequestDto requestDto)
    {
        var account = Require(accountId);

        account.UpdateProfile(requestDto.FirstName, requestDto.LastName, requestDto.Phone, requestDto.ContactEmail);
        accountRepository.Update(account);

        return account.ToProfileDto();
    }

    public ProfileDto SetImage(Guid accountId, SetProfileImageRequestDto requestDto)
    {
        var account = Require(accountId);

        account.SetProfileImage(requestDto.Key);
        accountRepository.Update(account);

        return account.ToProfileDto();
    }

    public void RemoveImage(Guid accountId)
    {
        var account = Require(accountId);

        account.RemoveProfileImage();
        accountRepository.Update(account);
    }

    public FileReference GetImageContent(Guid accountId)
    {
        if (accountRepository.GetById(accountId)?.Profile?.ImageUrl is not { } key)
            throw new NotFoundException("Profile image not found.");

        return new FileReference(key, null);
    }

    private Domain.Account Require(Guid accountId) =>
        accountRepository.GetById(accountId) ?? throw new NotFoundException("Account not found.");
}
