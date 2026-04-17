namespace Vela.Application.Common;

public enum ResultErrorType
{
    General,
    NotFound,
    Forbidden
}

public class Result
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public ResultErrorType ErrorType { get; }

    protected Result(bool success, string? errorMessage, ResultErrorType errorType = ResultErrorType.General)
    {
        Success = success;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string errorMessage, ResultErrorType errorType = ResultErrorType.General) => new(false, errorMessage, errorType);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool success, T? data, string? errorMessage, ResultErrorType errorType = ResultErrorType.General)
        : base(success, errorMessage, errorType)
    {
        Data = data;
    }

    public static Result<T> Ok(T data) => new(true, data, null);
    public new static Result<T> Fail(string errorMessage, ResultErrorType errorType = ResultErrorType.General) => new(false, default, errorMessage, errorType);
}
