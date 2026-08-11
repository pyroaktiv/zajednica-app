using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Infrastructure.Authentication;

namespace Zajednica.Api.Controllers.Identity;

[ApiController]
[Authorize]
[Route("api/devices")]
public sealed class DeviceController : ControllerBase
{
    private readonly IDeviceService _devices;

    public DeviceController(IDeviceService devices)
    {
        _devices = devices;
    }

    [HttpPost]
    public IActionResult Register([FromBody] RegisterDeviceRequestDto requestDto)
    {
        _devices.Register(User.AccountId(), requestDto);
        return Ok();
    }

    [HttpDelete]
    public IActionResult Unregister([FromBody] RegisterDeviceRequestDto requestDto)
    {
        _devices.Unregister(requestDto);
        return Ok();
    }
}
