namespace Vela.Application.Common;

public class Result
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    protected Result(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string errorMessage) => new(false, errorMessage);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool success, T? data, string? errorMessage)
        : base(success, errorMessage)
    {
        Data = data;
    }

    public static Result<T> Ok(T data) => new(true, data, null);
    public new static Result<T> Fail(string errorMessage) => new(false, default, errorMessage);
}