namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents an unauthorized access error.
/// Maps from UnauthorizedAccessException.
/// </summary>
public sealed class UnauthorizedError : BaseError, IDefaultMutationError
{
    /// <summary>
    /// Initializes a new instance of the UnauthorizedError class from an UnauthorizedAccessException.
    /// </summary>
    /// <param name="ex">The unauthorized access exception.</param>
    public UnauthorizedError(UnauthorizedAccessException ex)
        : base(ex.Message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the UnauthorizedError class.
    /// </summary>
    public UnauthorizedError()
    {
    }
}
