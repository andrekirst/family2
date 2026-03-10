using FamilyHub.Api.Common.Infrastructure.Validation;
using FluentValidation;
using FamilyHub.Api.Resources;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

/// <summary>
/// Hot Chocolate error filter that maps FluentValidation's ValidationException
/// to structured GraphQL errors with category-specific error codes.
/// Uses ValidatorCategory from CustomState to distinguish:
///   - Input → VALIDATION_ERROR
///   - Auth → AUTHORIZATION_ERROR
///   - Business → BUSINESS_VALIDATION_ERROR
/// Uses IStringLocalizer to localize the top-level messages.
/// Individual field-level error messages are localized at the validator level.
/// </summary>
public sealed class ValidationExceptionErrorFilter(
    IStringLocalizer<SharedResources> localizer) : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is not ValidationException validationException)
        {
            return error;
        }

        var failures = validationException.Errors.ToList();
        if (failures.Count == 0)
        {
            return error;
        }

        // Determine category from the first failure's CustomState
        // All failures in a single exception share the same category
        // (they come from the same validator group)
        var category = failures[0].CustomState is ValidatorCategory vc
            ? vc
            : ValidatorCategory.Input;

        var (code, message) = category switch
        {
            ValidatorCategory.Auth => ("AUTHORIZATION_ERROR", localizer["AuthorizationFailed"].Value),
            ValidatorCategory.Business => ("BUSINESS_VALIDATION_ERROR", localizer["BusinessValidationFailed"].Value),
            _ => ("VALIDATION_ERROR", localizer["ValidationFailed"].Value)
        };

        var errorDetails = failures
            .Select(e => new { e.PropertyName, e.ErrorMessage, e.ErrorCode })
            .ToList();

        return ErrorBuilder.New()
            .SetMessage(message)
            .SetCode(code)
            .SetExtension("validationErrors", errorDetails)
            .Build();
    }
}
