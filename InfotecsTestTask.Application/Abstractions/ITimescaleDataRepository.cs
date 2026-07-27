using InfotecsTestTask.Domain.Entities;

namespace InfotecsTestTask.Application.Abstractions;

public interface ITimescaleDataRepository
{
    Task ReplaceFileDataAsync(string fileName, IReadOnlyCollection<Values> values, Results result, CancellationToken cancellationToken);
}