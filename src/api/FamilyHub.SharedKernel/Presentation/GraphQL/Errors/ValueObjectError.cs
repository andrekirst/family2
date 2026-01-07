namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents a Vogen value object validation error.
/// Maps from ValueObjectValidationException.
/// </summary>
public sealed class ValueObjectError : BaseError
{
    public ValueObjectError(ValueObjectValidationException ex)
        : base(ex.Message)
    {
    }

    public ValueObjectError()
    {
    }
}
