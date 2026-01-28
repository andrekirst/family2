namespace FamilyHub.SharedKernel.Presentation.GraphQL.Errors;

/// <summary>
/// Represents an error when a requested resource is not found.
/// </summary>
/// <remarks>
/// <para>
/// Use this error when:
/// <list type="bullet">
/// <item><description>An entity lookup by ID returns null</description></item>
/// <item><description>A referenced resource doesn't exist</description></item>
/// <item><description>A Node query cannot resolve the global ID</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class NotFoundError : BaseError, IDefaultMutationError
{
    /// <summary>
    /// Gets or initializes the type of resource that was not found.
    /// </summary>
    /// <example>"User", "Family", "Invitation"</example>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Gets or initializes the identifier that was used in the lookup.
    /// </summary>
    public string? ResourceId { get; init; }

    /// <summary>
    /// Initializes a new instance of the NotFoundError class.
    /// </summary>
    public NotFoundError()
    {
    }

    /// <summary>
    /// Creates a NotFoundError for a specific resource type and ID.
    /// </summary>
    /// <param name="resourceType">The type of resource.</param>
    /// <param name="resourceId">The ID that was not found.</param>
    /// <returns>A new NotFoundError instance.</returns>
    public static NotFoundError For(string resourceType, string resourceId) => new()
    {
        Message = $"{resourceType} with ID '{resourceId}' was not found.",
        ResourceType = resourceType,
        ResourceId = resourceId
    };

    /// <summary>
    /// Creates a NotFoundError for a specific resource type and GUID.
    /// </summary>
    /// <param name="resourceType">The type of resource.</param>
    /// <param name="resourceId">The GUID that was not found.</param>
    /// <returns>A new NotFoundError instance.</returns>
    public static NotFoundError For(string resourceType, Guid resourceId)
        => For(resourceType, resourceId.ToString());
}
