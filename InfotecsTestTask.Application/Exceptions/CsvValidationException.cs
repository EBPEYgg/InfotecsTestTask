namespace InfotecsTestTask.Application.Exceptions;

public sealed class CsvValidationException : Exception
{
    public CsvValidationException(string message) : base(message)
    {
        Errors = [message];
    }

    public CsvValidationException(IReadOnlyCollection<string> errors) : base("CSV файл является недопустимым.")
    {
        Errors = errors;
    }

    public IReadOnlyCollection<string> Errors { get; }
}