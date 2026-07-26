namespace InfotecsTestTask.Domain.Entities;

public class Results
{
    /// <summary>
    /// Уникальный идентификатор интегрального результата.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Имя импортированного файла.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Дельта времени Date в секундах.
    /// </summary>
    public required double DateDeltaSeconds { get; set; }

    /// <summary>
    /// Момент запуска первой операции.
    /// </summary>
    public required DateTime FirstOperationDate { get; set; }

    /// <summary>
    /// Среднее время выполнения.
    /// </summary>
    public required double AverageExecutionTime { get; set; }

    /// <summary>
    /// Среднее значение показателя.
    /// </summary>
    public required double AverageValue { get; set; }

    /// <summary>
    /// Медиана по значениям показателя.
    /// </summary>
    public required double MedianValue { get; set; }

    /// <summary>
    /// Максимальное значение показателя.
    /// </summary>
    public required double MaxValue { get; set; }

    /// <summary>
    /// Минимальное значение показателя.
    /// </summary>
    public required double MinValue { get; set; }
}