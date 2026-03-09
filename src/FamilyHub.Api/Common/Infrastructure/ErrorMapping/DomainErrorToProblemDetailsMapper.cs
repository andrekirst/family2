using FamilyHub.Common.Application;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FamilyHub.Api.Common.Infrastructure.ErrorMapping;

/// <summary>
/// Maps <see cref="DomainError"/> to RFC 9457 ProblemDetails responses.
/// Uses consistent URN scheme: urn:familyhub:errors:{category}.
/// </summary>
public static class DomainErrorToProblemDetailsMapper
{
    private const string UrnPrefix = "urn:familyhub:errors";

    public static ProblemHttpResult ToProblemDetails(DomainError error)
    {
        var (statusCode, category) = error.Category switch
        {
            DomainErrorCategory.Validation => (StatusCodes.Status400BadRequest, "validation"),
            DomainErrorCategory.NotFound => (StatusCodes.Status404NotFound, "not-found"),
            DomainErrorCategory.Forbidden => (StatusCodes.Status403Forbidden, "forbidden"),
            DomainErrorCategory.Conflict => (StatusCodes.Status409Conflict, "conflict"),
            DomainErrorCategory.BusinessRule => (StatusCodes.Status422UnprocessableEntity, "business-rule"),
            _ => (StatusCodes.Status500InternalServerError, "unknown")
        };

        return TypedResults.Problem(
            title: GetTitle(error.Category),
            detail: error.Message,
            statusCode: statusCode,
            type: $"{UrnPrefix}:{category}",
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = error.ErrorCode
            });
    }

    private static string GetTitle(DomainErrorCategory category) => category switch
    {
        DomainErrorCategory.Validation => "Validation Error",
        DomainErrorCategory.NotFound => "Not Found",
        DomainErrorCategory.Forbidden => "Forbidden",
        DomainErrorCategory.Conflict => "Conflict",
        DomainErrorCategory.BusinessRule => "Business Rule Violation",
        _ => "Internal Server Error"
    };
}
