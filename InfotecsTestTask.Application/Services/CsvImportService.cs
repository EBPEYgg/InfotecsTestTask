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

    private const string DateFormat = "yyyy-MM-dd'T'HH-mm-ss.ffff'Z'";

    #endregion

    #region Методы

    public async Task<CsvImportResponse> ImportAsync(
        Stream csvStream, 
        string fileName, 
        CancellationToken cancellationToken)
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

        return new CsvImportResponse(fileName, values.Count, result.ToDto());
    }

    private async Task<IReadOnlyCollection<Values>> ParseAsync(
        Stream csvStream, 
        string fileName, 
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(csvStream);
        var header = await reader.ReadLineAsync(cancellationToken) ?? throw new CsvValidationException("CSV файл пустой.");
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
                errors.Add($"Максимальное количество строк в CSV файле: {MaxRowsCount}.");
                break;
            }

            var rowErrors = new List<string>();
            var parsedValue = ParseRow(line, rowNumber, fileName, rowErrors);

            if (rowErrors.Count > 0)
                errors.AddRange(rowErrors);
            else
                values.Add(parsedValue!);
        }

        if (values.Count < MinRowsCount)
        {
            errors.Add($"Минимальное количество строк в CSV файле: {MinRowsCount}.");
        }

        if (errors.Count > 0)
        {
            throw new CsvValidationException(errors);
        }

        return values;
    }

    private static void ValidateHeader(string header)
    {
        var columns = CleanCsvLine(header).Split(';', StringSplitOptions.TrimEntries);
        if (columns.Length != 3 ||
            columns[0] != "Date" ||
            columns[1] != "ExecutionTime" ||
            columns[2] != "Value")
        {
            throw new CsvValidationException("Заголовок в CSV файле должен быть: Date;ExecutionTime;Value");
        }
    }

    private Values? ParseRow(string line, int rowNumber, string fileName, ICollection<string> errors)
    {
        var columns = CleanCsvLine(line).Split(';', StringSplitOptions.TrimEntries);
        if (columns.Length != 3 || columns.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add($"Строка {rowNumber}: Date, ExecutionTime и Value являются обязательными.");
            return null;
        }

        var date = ParseDate(columns[0], rowNumber, errors);
        var executionTime = ParseNonNegativeDouble(columns[1], rowNumber, "ExecutionTime", errors);
        var value = ParseNonNegativeDouble(columns[2], rowNumber, "Value", errors);

        if (errors.Count > 0)
            return null;

        return new Values
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            Date = date!.Value,
            ExecutionTime = executionTime!.Value,
            Value = value!.Value
        };
    }

    private static string CleanCsvLine(string line) => line.Trim('"');

    private DateTime? ParseDate(string rawValue, int rowNumber, ICollection<string> errors)
    {
        if (!TryParseDate(rawValue, out var date))
        {
            errors.Add($"Строка {rowNumber}: дата имеет неверный формат.");
            return null;
        }

        if (date < MinAllowedDate || date > DateTime.UtcNow)
        {
            errors.Add($"Строка {rowNumber}: дата не может быть позже текущей (UTC) и раньше 01.01.2000.");
            return null;
        }

        return date;
    }

    private static double? ParseNonNegativeDouble(
        string rawValue, 
        int rowNumber, 
        string fieldName, 
        ICollection<string> errors)
    {
        if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            errors.Add($"Строка {rowNumber}: {fieldName} должно быть числом или цифрой.");
            return null;
        }

        if (value < 0)
        {
            errors.Add($"Строка {rowNumber}: {fieldName} не может быть меньше 0.");
            return null;
        }

        return value;
    }

    private static bool TryParseDate(string value, out DateTime date)
    {
        if (DateTime.TryParseExact(
                value,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out date))
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return true;
        }

        date = default;
        return false;
    }

    private static Results BuildResult(string fileName, IReadOnlyCollection<Values> values)
    {
        var orderedValues = values.Select(x => x.Value).Order().ToArray();
        
        var maxValue = orderedValues[^1];
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