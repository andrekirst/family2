namespace FamilyHub.Infrastructure.GraphQL.Directives;

/// <summary>
/// Represents the visibility level for a GraphQL field.
/// Used with the @visible directive to control field-level access.
/// </summary>
/// <remarks>
/// <para>
/// The visibility levels form a hierarchy where each level includes access from higher levels:
/// <list type="bullet">
/// <item><description>OWNER: Only the profile owner can see this field</description></item>
/// <item><description>FAMILY: Family members (including owner) can see this field</description></item>
/// <item><description>PUBLIC: All authenticated users can see this field</description></item>
/// </list>
/// </para>
/// <para>
/// This enum is used in the GraphQL schema directive. The actual runtime visibility
/// is determined by combining this with the user's ProfileFieldVisibility settings.
/// </para>
/// </remarks>
public enum FieldVisibility
{
    /// <summary>
    /// Field is only visible to the profile owner.
    /// The most restrictive visibility level.
    /// </summary>
    Owner,

    /// <summary>
    /// Field is visible to family members and the profile owner.
    /// This is the default for sensitive personal information.
    /// </summary>
    Family,

    /// <summary>
    /// Field is visible to all authenticated users.
    /// Used for publicly shareable information.
    /// </summary>
    Public
}
