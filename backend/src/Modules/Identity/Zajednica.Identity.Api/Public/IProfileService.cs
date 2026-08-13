using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Api.Public;

public interface IProfileService
{
    ProfileDto Get(Guid accountId);
    ProfileDto Update(Guid accountId, UpdateProfileRequestDto requestDto);
    ProfileDto SetImage(Guid accountId, SetProfileImageRequestDto requestDto);
    void RemoveImage(Guid accountId);
    FileReference GetImageContent(Guid accountId);
}
