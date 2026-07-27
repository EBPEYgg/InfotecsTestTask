using InfotecsTestTask.Application.Abstractions;
using InfotecsTestTask.Application.DTO;
using Microsoft.AspNetCore.Mvc;

namespace InfotecsTestTask.Web.Controllers;

[ApiController]
[Route("api/timescale-data")]
public sealed class TimescaleDataController(ICsvImportService csvImportService) : ControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CsvImportResponse>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var response = await csvImportService.ImportAsync(stream, file.FileName, cancellationToken);
        return Ok(response);
    }
}