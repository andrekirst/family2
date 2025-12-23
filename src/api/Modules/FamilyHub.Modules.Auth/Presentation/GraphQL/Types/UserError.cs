namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// Represents an error that occurred during a GraphQL mutation.
/// </summary>
public sealed record UserError
{
    /// <summary>
    /// Human-readable error message
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Machine-readable error code (e.g., "VALIDATION_ERROR", "OAUTH_ERROR")
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Field name that caused the error (null for general errors)
    /// </summary>
    public string? Field { get; init; }
}
