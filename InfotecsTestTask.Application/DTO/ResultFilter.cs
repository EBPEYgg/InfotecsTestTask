namespace InfotecsTestTask.Application.DTO;

public sealed record ResultFilter(
    string? FileName,
    DateTime? FirstOperationDateFrom,
    DateTime? FirstOperationDateTo,
    double? AverageValueFrom,
    double? AverageValueTo,
    double? AverageExecutionTimeFrom,
    double? AverageExecutionTimeTo);