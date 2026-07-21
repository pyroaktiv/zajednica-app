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

    public async Task<ProfileDto> GetAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await _accounts.GetByIdAsync(accountId, ct)
            ?? throw new NotFoundException("Account not found.");
        return account.ToProfileDto();
    }

    public async Task<ProfileDto> UpdateAsync(Guid accountId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var account = await _accounts.GetByIdAsync(accountId, ct)
            ?? throw new NotFoundException("Account not found.");

        account.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.ContactEmail, request.ImageUrl);
        await _accounts.UpdateAsync(account, ct);

        return account.ToProfileDto();
    }
}
