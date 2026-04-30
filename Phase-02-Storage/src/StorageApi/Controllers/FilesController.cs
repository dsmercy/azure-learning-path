using Microsoft.AspNetCore.Mvc;
using StorageApi.Models;
using StorageApi.Services;

namespace StorageApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController(IFileService fileService) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<FileUploadResponse>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest("File is empty.");
        var result = await fileService.UploadAsync(file, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> List(CancellationToken ct)
    {
        var blobs = await fileService.ListAsync(ct);
        return Ok(blobs);
    }

    [HttpGet("sas")]
    public async Task<ActionResult<string>> GetSasUrl([FromQuery] string blobName, CancellationToken ct)
    {
        var url = await fileService.GetSasUrlAsync(blobName, ct);
        return url is null ? NotFound() : Ok(url);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string blobName, CancellationToken ct)
    {
        var deleted = await fileService.DeleteAsync(blobName, ct);
        return deleted ? NoContent() : NotFound();
    }
}
