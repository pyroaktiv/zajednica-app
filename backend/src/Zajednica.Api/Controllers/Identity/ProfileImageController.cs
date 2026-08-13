using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Identity.Api.Public;

namespace Zajednica.Api.Controllers.Identity;

[ApiController]
[Authorize]
[Route("api/profiles/{accountId:guid}/image")]
public sealed class ProfileImageController : ControllerBase
{
    private readonly IProfileService _profiles;
    private readonly IFileStorage _storage;

    public ProfileImageController(IProfileService profiles, IFileStorage storage)
    {
        _profiles = profiles;
        _storage = storage;
    }

    [HttpGet]
    public IActionResult Get(Guid accountId)
    {
        var reference = _profiles.GetImageContent(accountId);
        var file = _storage.Open(reference.Key);
        if (file is null)
            return NotFound();

        Response.Headers.CacheControl = "private, max-age=300";
        return File(file.Content, file.ContentType, reference.DownloadName, enableRangeProcessing: true);
    }
}
