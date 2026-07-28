using InfotecsTestTask.Application.Abstractions;
using InfotecsTestTask.Application.DTO;
using Microsoft.AspNetCore.Mvc;

namespace InfotecsTestTask.Web.Controllers;

[ApiController]
[Route("api/timescale-data")]
public sealed class TimescaleDataController(ICsvImportService csvImportService, ITimescaleDataRepository repository) : ControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CsvImportResponse>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var response = await csvImportService.ImportAsync(stream, file.FileName, cancellationToken);
        return Ok(response);
    }

    [HttpGet("results")]
    public async Task<ActionResult<IReadOnlyCollection<ResultDto>>> GetResults([FromQuery] ResultFilter filter, CancellationToken cancellationToken)
    {
        var results = await repository.GetResultsAsync(filter, cancellationToken);
        return Ok(results);
    }

    [HttpGet("values/latest")]
    public async Task<ActionResult<IReadOnlyCollection<ValueDto>>> GetLastValues([FromQuery] string fileName, CancellationToken cancellationToken)
    {
        var values = await repository.GetLastTenValuesAsync(fileName, cancellationToken);
        return Ok(values);
    }
}