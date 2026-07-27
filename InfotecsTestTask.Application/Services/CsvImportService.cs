using InfotecsTestTask.Application.Abstractions;
using InfotecsTestTask.Application.DTO;
using InfotecsTestTask.Application.Exceptions;
using InfotecsTestTask.Application.Mappers;
using InfotecsTestTask.Domain.Entities;
using NLog;
using System.Globalization;

namespace InfotecsTestTask.Application.Services;

public class CsvImportService(ITimescaleDataRepository repository) : ICsvImportService
{
    #region Поля

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly DateTime MinAllowedDate = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private const int MinRowsCount = 1;

    private const int MaxRowsCount = 10_000;

    private static readonly string DateFormats = "yyyy-MM-dd'T'HH-mm-ss.FFFFFFF'Z'";

    #endregion

    #region Методы

    public async Task<CsvImportResponse> ImportAsync(Stream csvStream, string fileName, CancellationToken cancellationToken)
    {
        if (csvStream is null)
            throw new CsvValidationException("Наличие потока чтения CSV файла является обязательным.");

        fileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new CsvValidationException("Имя файла обязательно.");

        var values = await ParseAsync(csvStream, fileName, cancellationToken);
        var result = BuildResult(fileName, values);

        await repository.ReplaceFileDataAsync(fileName, values, result, cancellationToken);
        _logger.Info("Из файла {FileName} импортировано строк: {RowsCount}", fileName, values.Count);

        return new CsvImportResponse(fileName, values.Count, ResultsMapper.ToDto(result));
    }

    private async Task<IReadOnlyCollection<Values>> ParseAsync(Stream csvStream, string fileName, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(csvStream);
        var header = await reader.ReadLineAsync(cancellationToken) ?? throw new CsvValidationException("CSV file is empty.");
        ValidateHeader(header);

        var values = new List<Values>();
        var errors = new List<string>();
        var rowNumber = 1;
        var dataRowsCount = 0;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            rowNumber++;
            dataRowsCount++;

            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (dataRowsCount > MaxRowsCount)
            {
                errors.Add($"Максимальное количество строк CSV файла: {MaxRowsCount}.");
                break;
            }

            var parsed = TryParseRow(line, rowNumber, fileName, errors);
            if (parsed is not null)
            {
                values.Add(parsed);
            }
        }

        if (values.Count < MinRowsCount)
        {
            errors.Add($"Минимальное количество строк в файле CSV: {MinRowsCount}.");
        }

        if (errors.Count > 0)
        {
            throw new CsvValidationException(errors);
        }

        return values;
    }

    private static void ValidateHeader(string header)
    {
        string cleanHeader = header.Trim('"');
        var columns = cleanHeader.Split(';', StringSplitOptions.TrimEntries);
        if (columns.Length != 3 ||
            columns[0] != "Date" ||
            columns[1] != "ExecutionTime" ||
            columns[2] != "Value")
        {
            throw new CsvValidationException("Заголовок в CSV файле должен быть: Date;ExecutionTime;Value");
        }
    }

    private Values? TryParseRow(string line, int rowNumber, string fileName, ICollection<string> errors)
    {
        var rowErrorsCount = errors.Count;
        var clearLine = line.Trim('"');
        var columns = clearLine.Split(';', StringSplitOptions.TrimEntries);
        if (columns.Length != 3 || columns.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add($"Строка {rowNumber}: дата, время выполнения и значение показателя обязательны.");
            return null;
        }

        if (!TryParseDate(columns[0], out var date))
        {
            errors.Add($"Строка {rowNumber}: дата имеет неверный формат.");
        }
        else if (date < MinAllowedDate || date > DateTime.UtcNow)
        {
            errors.Add($"Строка {rowNumber}: дата не может быть позже текущей (UTC) и раньше 01.01.2000.");
        }

        if (!double.TryParse(columns[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var executionTime))
        {
            errors.Add($"Строка {rowNumber}: время выполнения должно быть числом или цифрой.");
        }
        else if (executionTime < 0)
        {
            errors.Add($"Строка {rowNumber}: время выполнения не может быть меньше 0.");
        }

        if (!double.TryParse(columns[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            errors.Add($"Строка {rowNumber}: значение показателя должно быть числом или цифрой.");
        }
        else if (value < 0)
        {
            errors.Add($"Строка {rowNumber}: значение показателя не может быть меньше 0.");
        }

        if (errors.Count != rowErrorsCount)
        {
            return null;
        }

        return new Values
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            Date = date,
            ExecutionTime = executionTime,
            Value = value
        };
    }

    private static bool TryParseDate(string value, out DateTime date)
    {
        if (DateTime.TryParseExact(
                value,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out date))
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return true;
        }

        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var offset))
        {
            date = offset.UtcDateTime;
            return true;
        }

        date = default;
        return false;
    }

    private static Results BuildResult(string fileName, IReadOnlyCollection<Values> values)
    {
        int count = values.Count;
        var orderedValues = values.Select(x => x.Value).Order().ToArray();
        
        var maxValue = orderedValues[count - 1];
        var minValue = orderedValues[0];

        var median = orderedValues.Length % 2 == 1
            ? orderedValues[orderedValues.Length / 2]
            : (orderedValues[orderedValues.Length / 2 - 1] + orderedValues[orderedValues.Length / 2]) / 2;

        DateTime maxDate = values.Max(x => x.Date);
        DateTime minDate = values.Min(x => x.Date);

        return new Results
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            DateDeltaSeconds = (maxDate - minDate).TotalSeconds,
            FirstOperationDate = minDate,
            AverageExecutionTime = values.Average(x => x.ExecutionTime),
            AverageValue = values.Average(x => x.Value),
            MedianValue = median,
            MaxValue = maxValue,
            MinValue = minValue
        };
    }

    #endregion
}