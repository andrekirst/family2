namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents a Vogen value object validation error.
/// Maps from ValueObjectValidationException.
/// </summary>
public sealed class ValueObjectError : BaseError, IDefaultMutationError
{
    /// <summary>
    /// Initializes a new instance of the ValueObjectError class from a ValueObjectValidationException.
    /// </summary>
    /// <param name="ex">The value object validation exception.</param>
    public ValueObjectError(ValueObjectValidationException ex)
        : base(ex.Message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValueObjectError class.
    /// </summary>
    public ValueObjectError()
    {
    }
}
