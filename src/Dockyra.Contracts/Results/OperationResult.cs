namespace Dockyra.Contracts.Results;

public enum ErrorCategory
{
    None = 0,
    Application = 1,
    Validation = 2,
    WindowsApi = 3,
    Persistence = 4,
    Module = 5,
    Ui = 6,
    Recovery = 7
}

public sealed record ErrorInfo(
    string Code,
    string Message,
    ErrorCategory Category = ErrorCategory.Application,
    string? TechnicalDetail = null);

public readonly record struct OperationResult(bool IsSuccess, ErrorInfo? Error = null)
{
    public static OperationResult Success() => new(true, null);

    public static OperationResult Failure(
        string code,
        string message,
        ErrorCategory category = ErrorCategory.Application,
        string? technicalDetail = null) =>
        new(false, new ErrorInfo(code, message, category, technicalDetail));
}

public readonly record struct OperationResult<T>(bool IsSuccess, T? Value, ErrorInfo? Error = null)
{
    public static OperationResult<T> Success(T value) => new(true, value, null);

    public static OperationResult<T> Failure(
        string code,
        string message,
        ErrorCategory category = ErrorCategory.Application,
        string? technicalDetail = null) =>
        new(false, default, new ErrorInfo(code, message, category, technicalDetail));
}

