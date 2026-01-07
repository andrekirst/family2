namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents a validation error from FluentValidation.
/// Maps from ValidationException.
/// </summary>
public sealed class ValidationError : BaseError
{
    public required string Field { get; init; }

    public static ValidationError From(FluentValidation.ValidationException ex)
    {
        var firstError = ex.Errors.FirstOrDefault();
        return new ValidationError
        {
            Message = firstError?.ErrorMessage ?? "Validation failed",
            Field = firstError?.PropertyName ?? string.Empty
        };
    }
}
