namespace FamilyHub.SharedKernel.Domain;

/// <summary>
/// Represents an optional value that may or may not exist.
/// Use instead of null to make absence explicit and type-safe.
/// </summary>
/// <typeparam name="T">The type of the contained value.</typeparam>
/// <remarks>
/// This is a functional programming pattern that helps prevent null reference exceptions
/// by making the presence or absence of a value explicit in the type system.
/// </remarks>
public readonly struct Maybe<T> : IEquatable<Maybe<T>>
{
    private readonly T? _value;
    private readonly bool _hasValue;

    private Maybe(T value)
    {
        _value = value;
        _hasValue = true;
    }

    /// <summary>
    /// Gets a Maybe with no value.
    /// </summary>
    public static Maybe<T> None => default;

    /// <summary>
    /// Creates a Maybe containing the specified value.
    /// </summary>
    /// <param name="value">The value to contain.</param>
    /// <returns>A Maybe containing the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static Maybe<T> Some(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Maybe<T>(value);
    }

    /// <summary>
    /// Creates a Maybe from a nullable value.
    /// Returns None if the value is null, otherwise Some(value).
    /// </summary>
    /// <param name="value">The nullable value.</param>
    /// <returns>None if null; otherwise Some(value).</returns>
    public static Maybe<T> From(T? value)
        => value is null ? None : new Maybe<T>(value);

    /// <summary>
    /// Gets a value indicating whether this Maybe contains a value.
    /// </summary>
    public bool HasValue => _hasValue;

    /// <summary>
    /// Gets a value indicating whether this Maybe is empty.
    /// </summary>
    public bool HasNoValue => !_hasValue;

    /// <summary>
    /// Gets the contained value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when Maybe has no value.</exception>
    public T Value => _hasValue
        ? _value!
        : throw new InvalidOperationException("Maybe has no value. Check HasValue before accessing Value.");

    /// <summary>
    /// Gets the value or returns the specified default.
    /// </summary>
    /// <param name="defaultValue">The default value to return if Maybe is empty.</param>
    /// <returns>The contained value or the default.</returns>
    public T GetValueOrDefault(T defaultValue = default!)
        => _hasValue ? _value! : defaultValue;

    /// <summary>
    /// Gets the value or throws the specified exception.
    /// </summary>
    /// <param name="exception">The exception to throw if Maybe is empty.</param>
    /// <returns>The contained value.</returns>
    public T GetValueOrThrow(Exception exception)
        => _hasValue ? _value! : throw exception;

    /// <summary>
    /// Gets the value or throws with the specified message.
    /// </summary>
    /// <param name="errorMessage">The error message for the exception.</param>
    /// <returns>The contained value.</returns>
    public T GetValueOrThrow(string errorMessage)
        => _hasValue ? _value! : throw new InvalidOperationException(errorMessage);

    /// <summary>
    /// Transforms the contained value using the specified function.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="mapper">The transformation function.</param>
    /// <returns>A Maybe containing the transformed value, or None if empty.</returns>
    public Maybe<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return _hasValue ? Maybe<TResult>.Some(mapper(_value!)) : Maybe<TResult>.None;
    }

    /// <summary>
    /// Transforms the contained value using the specified async function.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="mapper">The async transformation function.</param>
    /// <returns>A Maybe containing the transformed value, or None if empty.</returns>
    public async Task<Maybe<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        if (!_hasValue)
        {
            return Maybe<TResult>.None;
        }

        var result = await mapper(_value!);
        return Maybe<TResult>.Some(result);
    }

    /// <summary>
    /// Chains Maybe operations (flatMap/bind).
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="binder">The function returning a new Maybe.</param>
    /// <returns>The result of applying the binder, or None if empty.</returns>
    public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return _hasValue ? binder(_value!) : Maybe<TResult>.None;
    }

    /// <summary>
    /// Chains Maybe operations asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="binder">The async function returning a new Maybe.</param>
    /// <returns>The result of applying the binder, or None if empty.</returns>
    public async Task<Maybe<TResult>> BindAsync<TResult>(Func<T, Task<Maybe<TResult>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return _hasValue ? await binder(_value!) : Maybe<TResult>.None;
    }

    /// <summary>
    /// Executes an action if the Maybe has a value.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This Maybe for fluent chaining.</returns>
    public Maybe<T> Do(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_hasValue)
        {
            action(_value!);
        }
        return this;
    }

    /// <summary>
    /// Executes an action if the Maybe has no value.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This Maybe for fluent chaining.</returns>
    public Maybe<T> DoIfNone(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (!_hasValue)
        {
            action();
        }
        return this;
    }

    /// <summary>
    /// Converts this Maybe to a Result.
    /// </summary>
    /// <param name="errorMessage">The error message if Maybe is empty.</param>
    /// <returns>A successful Result with the value, or a failed Result.</returns>
    public Result<T> ToResult(string errorMessage)
        => _hasValue ? Result.Success(_value!) : Result.Failure<T>(errorMessage);

    /// <summary>
    /// Implicitly converts a value to a Maybe.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Maybe<T>(T? value)
        => value is null ? None : new Maybe<T>(value);

    /// <inheritdoc/>
    public bool Equals(Maybe<T> other)
    {
        if (!_hasValue && !other._hasValue)
        {
            return true;
        }

        if (_hasValue != other._hasValue)
        {
            return false;
        }

        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is Maybe<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => _hasValue ? _value?.GetHashCode() ?? 0 : 0;

    /// <inheritdoc/>
    public override string ToString()
        => _hasValue ? $"Some({_value})" : "None";

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Maybe<T> left, Maybe<T> right)
        => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Maybe<T> left, Maybe<T> right)
        => !left.Equals(right);
}

/// <summary>
/// Extension methods for Maybe operations.
/// </summary>
public static class MaybeExtensions
{
    /// <summary>
    /// Converts a nullable reference to a Maybe.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>A Maybe containing the value or None.</returns>
    public static Maybe<T> ToMaybe<T>(this T? value) where T : class
        => Maybe<T>.From(value);

    /// <summary>
    /// Converts a nullable value type to a Maybe.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>A Maybe containing the value or None.</returns>
    public static Maybe<T> ToMaybe<T>(this T? value) where T : struct
        => value.HasValue ? Maybe<T>.Some(value.Value) : Maybe<T>.None;

    /// <summary>
    /// Returns the first Maybe that has a value, or None if all are empty.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="maybes">The Maybes to check.</param>
    /// <returns>The first Maybe with a value, or None.</returns>
    public static Maybe<T> FirstOrNone<T>(this IEnumerable<Maybe<T>> maybes)
    {
        foreach (var maybe in maybes)
        {
            if (maybe.HasValue)
            {
                return maybe;
            }
        }
        return Maybe<T>.None;
    }

    /// <summary>
    /// Filters a sequence to only those with values and unwraps them.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="maybes">The Maybes to filter.</param>
    /// <returns>The values that exist.</returns>
    public static IEnumerable<T> Values<T>(this IEnumerable<Maybe<T>> maybes)
        => maybes.Where(m => m.HasValue).Select(m => m.Value);
}
