namespace FamilyHub.SharedKernel.Domain;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Initializes a new instance of the Result class.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error message if the operation failed.</param>
    protected Result(bool isSuccess, string error)
    {
        switch (isSuccess)
        {
            case true when !string.IsNullOrWhiteSpace(error):
                throw new InvalidOperationException("A successful result cannot have an error.");
            case false when string.IsNullOrWhiteSpace(error):
                throw new InvalidOperationException("A failed result must have an error message.");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="value">The result value.</param>
    /// <returns>A successful result containing the value.</returns>
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value returned by the operation.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the Result&lt;T&gt; class.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error message if the operation failed.</param>
    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
