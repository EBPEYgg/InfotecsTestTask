namespace InfotecsTestTask.Application.DTO;

public sealed record CsvImportResponse(string FileName, int RowsImported, ResultDto Result);