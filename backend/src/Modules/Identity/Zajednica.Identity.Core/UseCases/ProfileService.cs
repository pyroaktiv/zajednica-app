using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Mappers;

namespace Zajednica.Identity.Core.UseCases;

public sealed class ProfileService(IAccountRepository accountRepository, IFileUrlMapper urlMapper) : IProfileService
{
    public ProfileDto Get(Guid accountId)
    {
        var account = accountRepository.GetById(accountId)
            ?? throw new NotFoundException("Account not found.");
        return account.ToProfileDto(urlMapper);
    }

    public ProfileDto Update(Guid accountId, UpdateProfileRequestDto requestDto)
    {
        var account = accountRepository.GetById(accountId)
            ?? throw new NotFoundException("Account not found.");

        account.UpdateProfile(requestDto.FirstName, requestDto.LastName, requestDto.Phone, requestDto.ContactEmail,
            urlMapper.ToKey(requestDto.ImageUrl));
        accountRepository.Update(account);

        return account.ToProfileDto(urlMapper);
    }
}
