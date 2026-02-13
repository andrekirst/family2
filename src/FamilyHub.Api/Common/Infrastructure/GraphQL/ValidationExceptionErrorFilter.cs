using FluentValidation;
using FamilyHub.Api.Resources;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

/// <summary>
/// Hot Chocolate error filter that maps FluentValidation's ValidationException
/// to structured GraphQL errors with a VALIDATION_ERROR code.
/// Uses IStringLocalizer to localize the top-level "Validation failed" message.
/// Individual field-level error messages are localized at the validator level.
/// </summary>
public sealed class ValidationExceptionErrorFilter(
    IStringLocalizer<SharedResources> localizer) : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is ValidationException validationException)
        {
            var failures = validationException.Errors
                .Select(e => new { e.PropertyName, e.ErrorMessage })
                .ToList();

            return ErrorBuilder.New()
                .SetMessage(localizer["ValidationFailed"])
                .SetCode("VALIDATION_ERROR")
                .SetExtension("validationErrors", failures)
                .Build();
        }

        return error;
    }
}
