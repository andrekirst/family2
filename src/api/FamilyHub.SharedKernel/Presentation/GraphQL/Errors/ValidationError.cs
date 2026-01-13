namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents a validation error from FluentValidation.
/// Maps from ValidationException.
/// </summary>
public sealed class ValidationError : BaseError, IDefaultMutationError
{
    /// <summary>
    /// Gets or initializes the field that failed validation.
    /// </summary>
    public required string Field { get; init; }

    /// <summary>
    /// Creates a ValidationError from a FluentValidation exception.
    /// </summary>
    /// <param name="ex">The FluentValidation exception.</param>
    /// <returns>A new ValidationError instance.</returns>
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
