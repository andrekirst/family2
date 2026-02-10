using FluentValidation;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

/// <summary>
/// Hot Chocolate error filter that maps FluentValidation's ValidationException
/// to structured GraphQL errors with a VALIDATION_ERROR code.
/// </summary>
public sealed class ValidationExceptionErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is ValidationException validationException)
        {
            var failures = validationException.Errors
                .Select(e => new { e.PropertyName, e.ErrorMessage })
                .ToList();

            return ErrorBuilder.New()
                .SetMessage("Validation failed")
                .SetCode("VALIDATION_ERROR")
                .SetExtension("validationErrors", failures)
                .Build();
        }

        return error;
    }
}
