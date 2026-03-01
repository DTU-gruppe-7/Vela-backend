namespace Vela.Application.Common;

public class Result
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private Result(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static Result Ok() => new Result(true, null);
    public static Result Fail(string errorMessage) => new Result(false, errorMessage);
}
