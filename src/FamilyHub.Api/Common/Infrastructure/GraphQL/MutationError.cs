using FamilyHub.Common.Application;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

/// <summary>
/// GraphQL error type for mutation failures, mapped from DomainError.
/// Used in Hot Chocolate union return types: MutationResult = SuccessPayload | MutationError.
///
/// Example GraphQL schema:
///   union CreateFamilyResult = FamilyDto | MutationError
///   type MutationError { message: String!, errorCode: String!, category: ErrorCategory! }
/// </summary>
public sealed record MutationError(
    string Message,
    string ErrorCode,
    ErrorCategory Category)
{
    public static MutationError FromDomainError(DomainError error) =>
        new(error.Message, error.ErrorCode, MapCategory(error.Category));

    private static ErrorCategory MapCategory(DomainErrorCategory category) => category switch
    {
        DomainErrorCategory.Validation => ErrorCategory.VALIDATION,
        DomainErrorCategory.NotFound => ErrorCategory.NOT_FOUND,
        DomainErrorCategory.Conflict => ErrorCategory.CONFLICT,
        DomainErrorCategory.Forbidden => ErrorCategory.FORBIDDEN,
        DomainErrorCategory.BusinessRule => ErrorCategory.BUSINESS_RULE,
        _ => ErrorCategory.BUSINESS_RULE
    };
}

/// <summary>
/// GraphQL enum for error categories, using SCREAMING_SNAKE_CASE per GraphQL convention.
/// </summary>
public enum ErrorCategory
{
    VALIDATION,
    NOT_FOUND,
    CONFLICT,
    FORBIDDEN,
    BUSINESS_RULE
}
