namespace InfotecsTestTask.Application.DTO;

public sealed record ResultDto(
    Guid Id,
    string FileName,
    double DateDeltaSeconds,
    DateTime FirstOperationDate,
    double AverageExecutionTime,
    double AverageValue,
    double MedianValue,
    double MaxValue,
    double MinValue);