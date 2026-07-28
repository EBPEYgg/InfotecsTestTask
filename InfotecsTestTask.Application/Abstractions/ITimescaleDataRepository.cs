using InfotecsTestTask.Application.DTO;
using InfotecsTestTask.Domain.Entities;

namespace InfotecsTestTask.Application.Abstractions;

public interface ITimescaleDataRepository
{
    Task ReplaceFileDataAsync(string fileName, IReadOnlyCollection<Values> values, Results result, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ResultDto>> GetResultsAsync(ResultFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ValueDto>> GetLastTenValuesAsync(string fileName, CancellationToken cancellationToken);
}