namespace FamilyHub.Api.Application.Common;

public class OperationResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected OperationResult(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static OperationResult Success() => new(true, null);
    public static OperationResult Failure(string error) => new(false, error);
    public static OperationResult<T> Success<T>(T value) => new(value, true, null);
    public static OperationResult<T> Failure<T>(string error) => new(default!, false, error);
}

public class OperationResult<T> : OperationResult
{
    public T Value { get; }

    protected internal OperationResult(T value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static implicit operator OperationResult<T>(T value) => Success(value);
}
