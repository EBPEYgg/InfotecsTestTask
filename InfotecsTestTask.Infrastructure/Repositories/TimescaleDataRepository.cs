using InfotecsTestTask.Application.Abstractions;
using InfotecsTestTask.Application.DTO;
using InfotecsTestTask.Application.Mappers;
using InfotecsTestTask.Domain.Entities;
using InfotecsTestTask.Infrastructure.Persistence;
using InfotecsTestTask.Infrastructure.Persistence.QueryExtensions;
using Microsoft.EntityFrameworkCore;

namespace InfotecsTestTask.Infrastructure.Repositories;

public class TimescaleDataRepository(TimescaleDataDbContext dbContext) : ITimescaleDataRepository
{
    public async Task ReplaceFileDataAsync(
        string fileName, 
        IReadOnlyCollection<Values> values, 
        Results result, 
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Values.Where(x => x.FileName == fileName)
                              .ExecuteDeleteAsync(cancellationToken);

        await dbContext.Results.Where(x => x.FileName == fileName)
                               .ExecuteDeleteAsync(cancellationToken);

        await dbContext.Values.AddRangeAsync(values, cancellationToken);
        await dbContext.Results.AddAsync(result, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ResultDto>> GetResultsAsync(
        ResultFilter filter, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Results
            .AsNoTracking()
            .ApplyFilter(filter)
            .OrderBy(x => x.FileName)
            .ThenBy(x => x.FirstOperationDate)
            .Select(ResultsMapper.ToDtoExpression)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ValueDto>> GetLastTenValuesAsync(
        string fileName, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Values
            .AsNoTracking()
            .Where(x => x.FileName == fileName)
            .OrderByDescending(x => x.Date)
            .Take(10)
            .Select(ValuesMapper.ToDtoExpression)
            .ToArrayAsync(cancellationToken);
    }
}