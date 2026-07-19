using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Api.Public;

public interface IProfileService
{
    Task<ProfileDto> GetAsync(Guid accountId, CancellationToken ct = default);
    Task<ProfileDto> UpdateAsync(Guid accountId, UpdateProfileRequest request, CancellationToken ct = default);
}
