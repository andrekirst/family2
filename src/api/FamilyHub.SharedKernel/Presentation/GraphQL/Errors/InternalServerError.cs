namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents an unexpected internal error.
/// Maps from Exception (catch-all).
/// </summary>
public sealed class InternalServerError : BaseError
{
    public InternalServerError(Exception ex)
        : base("An unexpected error occurred. Please try again.")
    {
    }

    public InternalServerError()
    {
    }
}
