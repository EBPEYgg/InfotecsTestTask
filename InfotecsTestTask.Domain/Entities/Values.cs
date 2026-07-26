namespace InfotecsTestTask.Domain.Entities;

public class Values
{
    /// <summary>
    /// Уникальный идентификатор измерения.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Имя импортированного файла.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Время начала ГГГГ-ММ-ДДTчч-мм-сс.ммммZ.
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// Время выполнения в секундах.
    /// </summary>
    public required double ExecutionTime { get; set; }

    /// <summary>
    /// Показатель в виде числа с плавающей запятой.
    /// </summary>
    public required double Value { get; set; }
}