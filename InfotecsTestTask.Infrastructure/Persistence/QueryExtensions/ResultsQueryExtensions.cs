using InfotecsTestTask.Application.DTO;
using InfotecsTestTask.Application.Extensions;
using InfotecsTestTask.Domain.Entities;

namespace InfotecsTestTask.Infrastructure.Persistence.QueryExtensions;

public static class ResultsQueryExtensions
{
    public static IQueryable<Results> ApplyFilter(this IQueryable<Results> query, ResultFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.FileName))
            query = query.Where(x => x.FileName == filter.FileName);

        if (filter.FirstOperationDateFrom is { } dateFrom)
            query = query.Where(x => x.FirstOperationDate >= dateFrom.ToUtc());

        if (filter.FirstOperationDateTo is { } dateTo)
            query = query.Where(x => x.FirstOperationDate <= dateTo.ToUtc());

        if (filter.AverageValueFrom is { } avgFrom)
            query = query.Where(x => x.AverageValue >= avgFrom);

        if (filter.AverageValueTo is { } avgTo)
            query = query.Where(x => x.AverageValue <= avgTo);

        if (filter.AverageExecutionTimeFrom is { } execFrom)
            query = query.Where(x => x.AverageExecutionTime >= execFrom);

        if (filter.AverageExecutionTimeTo is { } execTo)
            query = query.Where(x => x.AverageExecutionTime <= execTo);

        return query;
    }
}