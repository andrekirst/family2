using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FamilyHub.Infrastructure.GraphQL.Directives;

/// <summary>
/// Default implementation of <see cref="IVisibilityContext"/> that uses HTTP context
/// to determine the current viewer and their access rights.
/// </summary>
/// <remarks>
/// <para>
/// This implementation extracts the current user ID and family ID from JWT claims
/// and determines field visibility based on the viewer's relationship to the profile owner.
/// </para>
/// <para>
/// Visibility rules:
/// <list type="bullet">
/// <item><description>OWNER level: Only the profile owner (UserId matches) can see the field</description></item>
/// <item><description>FAMILY level: Profile owner OR any family member (same FamilyId) can see the field</description></item>
/// <item><description>PUBLIC level: Any authenticated user can see the field</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class VisibilityContext : IVisibilityContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates a new instance of the visibility context.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public VisibilityContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public Guid? CurrentUserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");

            if (claim is null || !Guid.TryParse(claim.Value, out var userId))
            {
                return null;
            }

            return userId;
        }
    }

    /// <inheritdoc />
    public Guid? CurrentFamilyId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("family_id");

            if (claim is null || !Guid.TryParse(claim.Value, out var familyId))
            {
                return null;
            }

            return familyId;
        }
    }

    /// <inheritdoc />
    public Task<bool> CanViewFieldAsync(
        object? parentObject,
        string fieldName,
        FieldVisibility directiveVisibility,
        CancellationToken cancellationToken = default)
    {
        // Not authenticated - no access
        if (CurrentUserId is null)
        {
            return Task.FromResult(false);
        }

        // For PUBLIC visibility, any authenticated user can see
        if (directiveVisibility == FieldVisibility.PUBLIC)
        {
            return Task.FromResult(true);
        }

        // Extract owner information from the parent object
        var (ownerUserId, ownerFamilyId) = ExtractOwnerInfo(parentObject);

        // Check if viewer is the owner
        var isOwner = ownerUserId.HasValue && CurrentUserId == ownerUserId.Value;

        // OWNER visibility - only the profile owner can see
        if (directiveVisibility == FieldVisibility.OWNER)
        {
            return Task.FromResult(isOwner);
        }

        // FAMILY visibility - owner OR same family
        if (directiveVisibility == FieldVisibility.FAMILY)
        {
            if (isOwner)
            {
                return Task.FromResult(true);
            }

            // Check if viewer is in the same family
            var isFamilyMember = ownerFamilyId.HasValue
                && CurrentFamilyId.HasValue
                && CurrentFamilyId == ownerFamilyId.Value;

            return Task.FromResult(isFamilyMember);
        }

        // Default: deny access
        return Task.FromResult(false);
    }

    /// <summary>
    /// Extracts the owner's user ID and family ID from the parent object.
    /// </summary>
    /// <remarks>
    /// This method uses reflection to find common property names that indicate ownership.
    /// Supported patterns:
    /// - UserId property (for UserProfile, etc.)
    /// - OwnerId property (for Family, etc.)
    /// - Id property combined with FamilyId (for User)
    /// </remarks>
    private static (Guid? UserId, Guid? FamilyId) ExtractOwnerInfo(object? parentObject)
    {
        if (parentObject is null)
        {
            return (null, null);
        }

        var type = parentObject.GetType();
        Guid? userId = null;
        Guid? familyId = null;

        // Try to get UserId
        var userIdProp = type.GetProperty("UserId");
        if (userIdProp is not null)
        {
            var value = userIdProp.GetValue(parentObject);
            userId = ExtractGuidValue(value);
        }
        else
        {
            // Try OwnerId for Family-type objects
            var ownerIdProp = type.GetProperty("OwnerId");
            if (ownerIdProp is not null)
            {
                var value = ownerIdProp.GetValue(parentObject);
                userId = ExtractGuidValue(value);
            }
            else
            {
                // Try Id directly (for User type)
                var idProp = type.GetProperty("Id");
                if (idProp is not null)
                {
                    var value = idProp.GetValue(parentObject);
                    userId = ExtractGuidValue(value);
                }
            }
        }

        // Try to get FamilyId
        var familyIdProp = type.GetProperty("FamilyId");
        if (familyIdProp is not null)
        {
            var value = familyIdProp.GetValue(parentObject);
            familyId = ExtractGuidValue(value);
        }

        return (userId, familyId);
    }

    /// <summary>
    /// Extracts a Guid value from various types (Guid, Vogen value objects, etc.).
    /// </summary>
    private static Guid? ExtractGuidValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is Guid guid)
        {
            return guid;
        }

        // Handle Vogen value objects that wrap Guid
        var valueType = value.GetType();
        var valueProp = valueType.GetProperty("Value");
        if (valueProp is not null && valueProp.PropertyType == typeof(Guid))
        {
            return (Guid?)valueProp.GetValue(value);
        }

        return null;
    }
}
