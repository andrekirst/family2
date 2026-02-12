namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

/// <summary>
/// Hot Chocolate error filter that maps InvalidOperationException
/// to structured GraphQL errors with a BUSINESS_LOGIC_ERROR code.
/// Prevents Hot Chocolate from masking domain logic errors as "Unexpected Execution Error".
/// </summary>
public sealed class BusinessLogicExceptionErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is InvalidOperationException ex)
        {
            return ErrorBuilder.New()
                .SetMessage(ex.Message)
                .SetCode("BUSINESS_LOGIC_ERROR")
                .Build();
        }

        return error;
    }
}
