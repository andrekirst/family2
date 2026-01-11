using FamilyHub.SharedKernel.Domain.Exceptions;

namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents a business logic error.
/// Maps from BusinessException.
/// </summary>
public sealed class BusinessError : BaseError
{
    /// <summary>
    /// Gets or initializes the business error code.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Initializes a new instance of the BusinessError class from a BusinessException.
    /// </summary>
    /// <param name="ex">The business exception.</param>
    public BusinessError(BusinessException ex)
        : base(ex.Message)
    {
        Code = ex.Code;
    }

    /// <summary>
    /// Initializes a new instance of the BusinessError class.
    /// </summary>
    public BusinessError()
    {
    }
}
