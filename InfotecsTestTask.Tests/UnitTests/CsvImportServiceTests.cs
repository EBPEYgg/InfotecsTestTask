using InfotecsTestTask.Application.Abstractions;
using InfotecsTestTask.Application.Exceptions;
using InfotecsTestTask.Application.Services;
using InfotecsTestTask.Domain.Entities;
using System.Text;

namespace InfotecsTestTask.Tests.UnitTests;

public sealed class CsvImportServiceTests
{
    private const string CsvHeader = "Date;ExecutionTime;Value";

    [Fact]
    public async Task ImportAsync_ValidCsv_CalculatesRoundedResponseAndSavesData()
    {
        var repository = new Mock<ITimescaleDataRepository>();
        var service = CreateService(repository);
        await using var stream = ToStream(
            $"""
            {CsvHeader}
            2024-01-01T10-00-00.0000Z;10.111;2.555
            2024-01-01T10-00-05.0000Z;20.222;6.555
            2024-01-01T10-00-10.0000Z;30.333;10.555
            """);

        var response = await service.ImportAsync(stream, "sample.csv", CancellationToken.None);

        response.FileName.Should().Be("sample.csv");
        response.RowsImported.Should().Be(3);
        response.Result.DateDeltaSeconds.Should().Be(10);
        response.Result.FirstOperationDate.Should().Be(new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        response.Result.AverageExecutionTime.Should().Be(20.222);
        response.Result.AverageValue.Should().Be(6.555);
        response.Result.MedianValue.Should().Be(6.555);
        response.Result.MaxValue.Should().Be(10.555);
        response.Result.MinValue.Should().Be(2.555);

        repository.Verify(
            x => x.ReplaceFileDataAsync(
                "sample.csv",
                It.Is<IReadOnlyCollection<Values>>(values => values.Count == 3),
                It.Is<Results>(result =>
                    result.FileName == "sample.csv" &&
                    Math.Abs(result.AverageValue - 6.555) < 0.000001),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ImportAsync_NegativeValue_ThrowsValidationExceptionAndDoesNotSave()
    {
        var repository = new Mock<ITimescaleDataRepository>();
        var service = CreateService(repository);
        await using var stream = ToStream(
            $"""
            {CsvHeader}
            2024-01-01T10-00-00.0000Z;10;-1
            """);

        var action = () => service.ImportAsync(stream, "invalid.csv", CancellationToken.None);

        var exception = await action.Should().ThrowAsync<CsvValidationException>();
        exception.Which.Errors.Should().Contain(x => x.Contains("Value"));
        VerifyDataWasNotSaved(repository);
    }

    [Fact]
    public async Task ImportAsync_EmptyData_ThrowsValidationException()
    {
        var repository = new Mock<ITimescaleDataRepository>();
        var service = CreateService(repository);
        await using var stream = ToStream("Date;ExecutionTime;Value");

        var action = () => service.ImportAsync(stream, "empty.csv", CancellationToken.None);

        var exception = await action.Should().ThrowAsync<CsvValidationException>();
        exception.Which.Errors.Should().Contain(x => x.Contains("Минимальное количество строк"));
        VerifyDataWasNotSaved(repository);
    }

    [Fact]
    public async Task ImportAsync_FutureDate_ThrowsValidationException()
    {
        var repository = new Mock<ITimescaleDataRepository>();
        var service = CreateService(repository);
        await using var stream = ToStream(
            $"""
            {CsvHeader}
            2999-01-01T00-00-00.0000Z;1;1
            """);

        var action = () => service.ImportAsync(stream, "future.csv", CancellationToken.None);

        var exception = await action.Should().ThrowAsync<CsvValidationException>();
        exception.Which.Errors.Should().Contain(x => x.Contains("дата не может быть позже текущей (UTC)"));
        VerifyDataWasNotSaved(repository);
    }

    [Fact]
    public async Task ImportAsync_MoreThanTenThousandRows_ThrowsValidationException()
    {
        var repository = new Mock<ITimescaleDataRepository>();
        var service = CreateService(repository);
        await using var stream = ToStream(CreateCsvWithRows(10_001));

        var action = () => service.ImportAsync(stream, "too-many.csv", CancellationToken.None);

        var exception = await action.Should().ThrowAsync<CsvValidationException>();
        exception.Which.Errors.Should().Contain(x => x.Contains("Максимальное количество строк"));
        VerifyDataWasNotSaved(repository);
    }

    private static CsvImportService CreateService(Mock<ITimescaleDataRepository> repository) => new(repository.Object);

    private static void VerifyDataWasNotSaved(Mock<ITimescaleDataRepository> repository) => repository.VerifyNoOtherCalls();

    private static MemoryStream ToStream(string content) => new(Encoding.UTF8.GetBytes(content));

    private static string CreateCsvWithRows(int rowsCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{CsvHeader}");

        for (var i = 0; i < rowsCount; i++)
        {
            builder.AppendLine($"2024-01-01T10-00-00.0000Z;{i};{i}");
        }

        return builder.ToString();
    }
}