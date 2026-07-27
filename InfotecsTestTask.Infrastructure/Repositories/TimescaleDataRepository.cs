using InfotecsTestTask.Application.Abstractions;
using InfotecsTestTask.Domain.Entities;
using InfotecsTestTask.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfotecsTestTask.Infrastructure.Repositories;

public class TimescaleDataRepository(TimescaleDataDbContext dbContext) : ITimescaleDataRepository
{
    public async Task ReplaceFileDataAsync(string fileName, IReadOnlyCollection<Values> values, Results result, CancellationToken cancellationToken)
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
}