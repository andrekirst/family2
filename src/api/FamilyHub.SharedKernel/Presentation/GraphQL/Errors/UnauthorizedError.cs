namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents an unauthorized access error.
/// Maps from UnauthorizedAccessException.
/// </summary>
public sealed class UnauthorizedError : BaseError
{
    public UnauthorizedError(UnauthorizedAccessException ex)
        : base(ex.Message)
    {
    }

    public UnauthorizedError()
    {
    }
}
