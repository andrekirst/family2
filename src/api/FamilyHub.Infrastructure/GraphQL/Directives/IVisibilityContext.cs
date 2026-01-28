namespace FamilyHub.Infrastructure.GraphQL.Directives;

/// <summary>
/// Provides context for field visibility decisions at runtime.
/// </summary>
/// <remarks>
/// <para>
/// This service is used by the VisibilityFieldMiddleware to determine if the current
/// viewer has access to a field based on the @visible directive and the viewer's
/// relationship to the resource owner.
/// </para>
/// <para>
/// Implementations should consider:
/// <list type="bullet">
/// <item><description>The current user's identity (from HttpContext/claims)</description></item>
/// <item><description>The profile owner's identity (from the parent object)</description></item>
/// <item><description>The family membership relationship between viewer and owner</description></item>
/// <item><description>The user's configured field visibility settings</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IVisibilityContext
{
    /// <summary>
    /// Determines if the current viewer can see a field on the given resource.
    /// </summary>
    /// <param name="parentObject">The parent object containing the field (e.g., UserProfile).</param>
    /// <param name="fieldName">The name of the field being accessed.</param>
    /// <param name="directiveVisibility">The visibility level specified by the @visible directive.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the viewer can see the field; otherwise, false.</returns>
    Task<bool> CanViewFieldAsync(
        object? parentObject,
        string fieldName,
        FieldVisibility directiveVisibility,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current viewer's user ID.
    /// </summary>
    Guid? CurrentUserId { get; }

    /// <summary>
    /// Gets the current viewer's family ID.
    /// </summary>
    Guid? CurrentFamilyId { get; }
}
