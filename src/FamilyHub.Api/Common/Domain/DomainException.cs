namespace FamilyHub.Api.Common.Domain;

/// <summary>
/// Exception thrown when a domain rule is violated.
/// Represents business logic violations rather than technical errors.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
