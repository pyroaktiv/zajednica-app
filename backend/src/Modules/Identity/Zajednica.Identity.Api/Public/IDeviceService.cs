using Zajednica.Identity.Api.Dto;

namespace Zajednica.Identity.Api.Public;

public interface IDeviceService
{
    void Register(Guid accountId, RegisterDeviceRequestDto requestDto);
    void Unregister(RegisterDeviceRequestDto requestDto);
}
