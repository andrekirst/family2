namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents a conflict error when an operation cannot be completed
/// due to a state conflict with existing data.
/// </summary>
/// <remarks>
/// <para>
/// Use this error when:
/// <list type="bullet">
/// <item><description>Attempting to create a resource that already exists</description></item>
/// <item><description>Optimistic concurrency check fails</description></item>
/// <item><description>A unique constraint would be violated</description></item>
/// <item><description>The resource is in an incompatible state for the operation</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ConflictError : BaseError, IDefaultMutationError
{
    /// <summary>
    /// Gets or initializes the type of resource that caused the conflict.
    /// </summary>
    /// <example>"User", "Family", "Email"</example>
    public required string ConflictingResource { get; init; }

    /// <summary>
    /// Gets or initializes the specific field or constraint that caused the conflict.
    /// </summary>
    public string? ConflictingField { get; init; }

    /// <summary>
    /// Initializes a new instance of the ConflictError class.
    /// </summary>
    public ConflictError()
    {
    }

    /// <summary>
    /// Creates a ConflictError for a duplicate resource.
    /// </summary>
    /// <param name="resourceType">The type of resource.</param>
    /// <param name="field">The field that caused the conflict.</param>
    /// <param name="value">The conflicting value.</param>
    /// <returns>A new ConflictError instance.</returns>
    public static ConflictError Duplicate(string resourceType, string field, string value) => new()
    {
        Message = $"A {resourceType} with {field} '{value}' already exists.",
        ConflictingResource = resourceType,
        ConflictingField = field
    };

    /// <summary>
    /// Creates a ConflictError for an invalid state transition.
    /// </summary>
    /// <param name="resourceType">The type of resource.</param>
    /// <param name="currentState">The current state.</param>
    /// <param name="requestedAction">The action that was attempted.</param>
    /// <returns>A new ConflictError instance.</returns>
    public static ConflictError InvalidState(string resourceType, string currentState, string requestedAction) => new()
    {
        Message = $"Cannot {requestedAction} {resourceType}: current state is '{currentState}'.",
        ConflictingResource = resourceType,
        ConflictingField = "Status"
    };
}
