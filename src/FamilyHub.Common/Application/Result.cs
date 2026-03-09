namespace FamilyHub.Common.Application;

/// <summary>
/// Discriminated result type for command/query handlers.
/// Replaces exception-based error handling with explicit success/failure paths.
/// Handlers return Result&lt;T&gt; instead of throwing DomainException.
///
/// Usage in handlers:
///   return Result&lt;FamilyDto&gt;.Success(dto);
///   return Result&lt;FamilyDto&gt;.Failure(DomainError.NotFound("FAMILY_NOT_FOUND", "Family not found"));
///
/// Usage in GraphQL mutation types:
///   Maps to Hot Chocolate union types for type-safe client error handling.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly DomainError? _error;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(DomainError error)
    {
        _error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result. Check IsSuccess first.");

    public DomainError Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful result. Check IsFailure first.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(DomainError error) => new(error);

    /// <summary>
    /// Implicit conversion from T to successful Result.
    /// Allows handlers to return the value directly: return myDto;
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from DomainError to failed Result.
    /// Allows handlers to return errors directly: return DomainError.Conflict(...);
    /// </summary>
    public static implicit operator Result<T>(DomainError error) => Failure(error);

    /// <summary>
    /// Pattern match on the result, executing the appropriate function.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainError, TResult> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(_error!);
}
