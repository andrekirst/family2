using FamilyHub.SharedKernel.Domain.Exceptions;

namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents a business logic error.
/// Maps from BusinessException.
/// </summary>
public sealed class BusinessError : BaseError
{
    public required string Code { get; init; }

    public BusinessError(BusinessException ex)
        : base(ex.Message)
    {
        Code = ex.Code;
    }

    public BusinessError()
    {
    }
}
