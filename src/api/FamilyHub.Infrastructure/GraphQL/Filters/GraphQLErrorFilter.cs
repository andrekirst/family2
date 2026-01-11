using FluentValidation;
using HotChocolate;

namespace FamilyHub.Infrastructure.GraphQL.Filters;

/// <summary>
/// Centralized GraphQL error filter for Hot Chocolate.
/// Maps exceptions to GraphQL errors with appropriate error codes.
/// Eliminates duplicated try-catch blocks across mutations/queries.
/// </summary>
public sealed class GraphQLErrorFilter : IErrorFilter
{
    /// <summary>
    /// Processes and transforms GraphQL errors based on the exception type.
    /// </summary>
    /// <param name="error">The GraphQL error to process.</param>
    /// <returns>A transformed error with appropriate error code and message.</returns>
    public IError OnError(IError error)
    {
        return error.Exception switch
        {
            ValidationException validationException => HandleValidationException(error, validationException),
            InvalidOperationException invalidOperationException => HandleInvalidOperationException(error, invalidOperationException),
            UnauthorizedAccessException unauthorizedAccessException => HandleUnauthorizedAccessException(error, unauthorizedAccessException),
            ArgumentException argumentException => HandleArgumentException(error, argumentException),
            _ => HandleUnknownException(error)
        };
    }

    private static IError HandleValidationException(IError error, ValidationException exception)
    {
        // FluentValidation errors - map to multiple GraphQL errors (one per field)
        var errorBuilder = ErrorBuilder.FromError(error)
            .SetMessage(exception.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed")
            .SetCode("VALIDATION_ERROR");

        // Add field-level errors as extensions
        if (exception.Errors.Any())
        {
            errorBuilder.SetExtension("validationErrors", exception.Errors.Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage
            }));
        }

        return errorBuilder.Build();
    }

    private static IError HandleInvalidOperationException(IError error, InvalidOperationException exception)
    {
        // Business rule violations (e.g., "User already has a family")
        return ErrorBuilder.FromError(error)
            .SetMessage(exception.Message)
            .SetCode("BUSINESS_ERROR")
            .Build();
    }

    private static IError HandleUnauthorizedAccessException(IError error, UnauthorizedAccessException exception)
    {
        // Authentication/authorization errors
        return ErrorBuilder.FromError(error)
            .SetMessage(exception.Message)
            .SetCode("UNAUTHENTICATED")
            .Build();
    }

    private static IError HandleArgumentException(IError error, ArgumentException exception)
    {
        // Domain validation errors (e.g., Vogen value object validation)
        var errorBuilder = ErrorBuilder.FromError(error)
            .SetMessage(exception.Message)
            .SetCode("ARGUMENT_ERROR");

        // Add parameter name if available
        if (!string.IsNullOrEmpty(exception.ParamName))
        {
            errorBuilder.SetExtension("parameterName", exception.ParamName);
        }

        return errorBuilder.Build();
    }

    private static IError HandleUnknownException(IError error)
    {
        // Unexpected errors - log but don't expose internal details
        return ErrorBuilder.FromError(error)
            .SetMessage("An unexpected error occurred. Please try again.")
            .SetCode("INTERNAL_ERROR")
            .Build();
    }
}
