using FamilyHub.Common.Domain;
using FamilyHub.Api.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Hosting;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

/// <summary>
/// Hot Chocolate error filter that maps DomainException and InvalidOperationException
/// to structured GraphQL errors with a BUSINESS_LOGIC_ERROR code.
/// When a DomainException has an ErrorCode, the filter maps it to a localized message
/// via IStringLocalizer, falling back to the exception message if no translation exists.
/// </summary>
public sealed class BusinessLogicExceptionErrorFilter(
    IStringLocalizer<DomainErrors> localizer,
    IWebHostEnvironment env) : IErrorFilter
{
    public IError OnError(IError error)
    {
        switch (error.Exception)
        {
            case ConflictException conflictEx:
                return ErrorBuilder.New()
                    .SetMessage(conflictEx.Message)
                    .SetCode("CONFLICT")
                    .SetExtension("errorCode", conflictEx.ErrorCode)
                    .SetExtension("entityType", conflictEx.EntityType)
                    .Build();
            case DomainException domainEx:
            {
                var message = GetLocalizedMessage(domainEx);

                return ErrorBuilder.New()
                    .SetMessage(message)
                    .SetCode("BUSINESS_LOGIC_ERROR")
                    .SetExtension("errorCode", domainEx.ErrorCode)
                    .Build();
            }
            case InvalidOperationException ex:
                return ErrorBuilder.New()
                    .SetMessage(env.IsDevelopment() ? ex.Message : "An internal error occurred.")
                    .SetCode("BUSINESS_LOGIC_ERROR")
                    .Build();
            default:
                return error;
        }
    }

    private string GetLocalizedMessage(DomainException ex)
    {
        if (ex.ErrorCode is null)
        {
            return ex.Message;
        }

        var localized = localizer[ex.ErrorCode];
        // If the key is not found, IStringLocalizer returns the key itself — fall back to exception message
        return localized.ResourceNotFound ? ex.Message : localized.Value;
    }
}
