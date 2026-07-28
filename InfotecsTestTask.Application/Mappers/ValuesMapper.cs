using InfotecsTestTask.Application.DTO;
using InfotecsTestTask.Domain.Entities;
using System.Linq.Expressions;

namespace InfotecsTestTask.Application.Mappers;

public static class ValuesMapper
{
    public static readonly Expression<Func<Values, ValueDto>> ToDtoExpression =
        entity => new ValueDto(
            entity.Id,
            entity.FileName,
            entity.Date,
            entity.ExecutionTime,
            entity.Value);
}