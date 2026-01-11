namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Base class for all GraphQL error types.
/// </summary>
public abstract class BaseError
{
    /// <summary>
    /// Gets or initializes the error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Initializes a new instance of the BaseError class.
    /// </summary>
    protected BaseError()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseError class with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    protected BaseError(string message)
    {
        Message = message;
    }
}
