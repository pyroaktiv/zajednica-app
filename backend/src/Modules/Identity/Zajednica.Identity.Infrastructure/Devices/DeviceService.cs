using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Infrastructural.RepositoryInterfaces;

namespace Zajednica.Identity.Infrastructure.Devices;

internal sealed class DeviceService(IDeviceTokenRepository repository) : IDeviceService
{
    public void Register(Guid accountId, RegisterDeviceRequestDto requestDto) =>
        repository.Save(accountId, requestDto.Token, DateTime.UtcNow);

    public void Unregister(RegisterDeviceRequestDto requestDto) =>
        repository.RemoveByToken(requestDto.Token);
}
