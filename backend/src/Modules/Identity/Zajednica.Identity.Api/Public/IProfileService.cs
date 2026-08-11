using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Api.Public;

public interface IProfileService
{
    ProfileDto Get(Guid accountId);
    ProfileDto Update(Guid accountId, UpdateProfileRequestDto requestDto);
}
