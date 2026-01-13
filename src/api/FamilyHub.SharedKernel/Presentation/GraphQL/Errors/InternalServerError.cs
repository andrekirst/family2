namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents an unexpected internal error.
/// Maps from Exception (catch-all).
/// </summary>
public sealed class InternalServerError : BaseError, IDefaultMutationError
{
    /// <summary>
    /// Initializes a new instance of the InternalServerError class from an Exception.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    public InternalServerError(Exception ex)
        : base("An unexpected error occurred. Please try again.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the InternalServerError class.
    /// </summary>
    public InternalServerError()
    {
    }
}
