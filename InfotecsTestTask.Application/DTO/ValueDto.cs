namespace InfotecsTestTask.Application.DTO;

public sealed record ValueDto(Guid Id, string FileName, DateTime Date, double ExecutionTime, double Value);