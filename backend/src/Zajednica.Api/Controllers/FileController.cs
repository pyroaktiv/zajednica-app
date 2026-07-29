using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Storage;

namespace Zajednica.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/files")]
public sealed class FileController : ControllerBase
{
    private readonly IFileStorage _storage;

    public FileController(IFileStorage storage)
    {
        _storage = storage;
    }

    [HttpPost("images")]
    [RequestSizeLimit(FileKind.ImageMaxBytes)]
    public ActionResult<UploadedFileDto> UploadImage(IFormFile file)
    {
        return Ok(Store(FileKind.Image, file));
    }

    [HttpPost("audio")]
    [RequestSizeLimit(FileKind.AudioMaxBytes)]
    public ActionResult<UploadedFileDto> UploadAudio(IFormFile file)
    {
        return Ok(Store(FileKind.Audio, file));
    }

    [HttpPost("documents")]
    [RequestSizeLimit(FileKind.DocumentMaxBytes)]
    public ActionResult<UploadedFileDto> UploadDocument(IFormFile file)
    {
        return Ok(Store(FileKind.Document, file));
    }

    private UploadedFileDto Store(FileKind kind, IFormFile file)
    {
        if (file is null)
            throw new EntityValidationException("A file is required.");

        var stored = kind.AcceptUpload(file.FileName, file.Length);

        using var content = file.OpenReadStream();
        return new UploadedFileDto(_storage.Save(content, stored));
    }
}
