using InfotecsTestTask.Application.DTO;
using InfotecsTestTask.Domain.Entities;

namespace InfotecsTestTask.Application.Mappers;

public static class ResultsMapper
{
    public static ResultDto ToDto(this Results entity)
    {
        return new ResultDto(
            entity.Id,
            entity.FileName,
            entity.DateDeltaSeconds,
            entity.FirstOperationDate,
            entity.AverageExecutionTime,
            entity.AverageValue,
            entity.MedianValue,
            entity.MaxValue,
            entity.MinValue);
    }

    public static Results ToEntity(this ResultDto entity)
    {
        return new Results
        {
            Id = entity.Id,
            FileName = entity.FileName,
            DateDeltaSeconds = entity.DateDeltaSeconds,
            FirstOperationDate = entity.FirstOperationDate,
            AverageExecutionTime = entity.AverageExecutionTime,
            AverageValue = entity.AverageValue,
            MedianValue = entity.MedianValue,
            MaxValue = entity.MaxValue,
            MinValue = entity.MinValue
        };
    }
}