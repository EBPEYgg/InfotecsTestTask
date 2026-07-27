using InfotecsTestTask.Application.DTO;

namespace InfotecsTestTask.Application.Abstractions;

public interface ICsvImportService
{
    Task<CsvImportResponse> ImportAsync(Stream csvStream, string fileName, CancellationToken cancellationToken);
}