using InfotecsTestTask.Application.DTO;
using InfotecsTestTask.Domain.Entities;
using System.Linq.Expressions;

namespace InfotecsTestTask.Application.Mappers;

public static class ResultsMapper
{
    public static readonly Expression<Func<Results, ResultDto>> ToDtoExpression =
        entity => new ResultDto(
            entity.Id,
            entity.FileName,
            entity.DateDeltaSeconds,
            entity.FirstOperationDate,
            entity.AverageExecutionTime,
            entity.AverageValue,
            entity.MedianValue,
            entity.MaxValue,
            entity.MinValue);

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
}