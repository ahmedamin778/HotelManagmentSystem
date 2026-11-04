using HotelBookingSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Controllers;

[ApiController]
[Route("api/damage-reports")]
public class DamageReportsController : ControllerBase
{
    private readonly DamageReportFileService _fileService;

    public DamageReportsController(DamageReportFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fileService.SaveAsync(file, cancellationToken);

            return CreatedAtRoute(
                "DownloadDamageReport",
                new { id = result.Id },
                result);
        }
        catch (DamageReportValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DamageReportQuotaExceededException ex)
        {
            return StatusCode(
                StatusCodes.Status507InsufficientStorage,
                new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/download", Name = "DownloadDamageReport")]
    public IActionResult Download(Guid id)
    {
        var file = _fileService.Find(id);

        if (file is null)
            return NotFound(new { message = "Damage report file was not found." });

        return PhysicalFile(file.Path, file.ContentType, file.DownloadName);
    }
}
