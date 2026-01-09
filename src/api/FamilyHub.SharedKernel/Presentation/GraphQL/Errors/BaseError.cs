namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

public abstract class BaseError
{
    public required string Message { get; init; }

    protected BaseError()
    {
    }

    protected BaseError(string message)
    {
        Message = message;
    }
}
