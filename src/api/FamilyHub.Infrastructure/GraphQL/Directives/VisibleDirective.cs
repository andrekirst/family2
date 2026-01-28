using HotChocolate.Types;

namespace FamilyHub.Infrastructure.GraphQL.Directives;

/// <summary>
/// Defines the @visible directive for controlling field-level visibility in the GraphQL schema.
/// </summary>
/// <remarks>
/// <para>
/// Usage in schema:
/// <code>
/// type UserProfile {
///   displayName: String! @visible(to: PUBLIC)
///   birthday: Date @visible(to: FAMILY)
///   preferences: ProfilePreferences! @visible(to: OWNER)
/// }
/// </code>
/// </para>
/// <para>
/// The directive specifies the MAXIMUM visibility level for a field. The actual runtime
/// visibility is determined by the intersection of:
/// <list type="bullet">
/// <item><description>The directive's visibility level (schema default)</description></item>
/// <item><description>The user's ProfileFieldVisibility settings (user preference)</description></item>
/// <item><description>The viewer's relationship to the profile owner (runtime context)</description></item>
/// </list>
/// </para>
/// <para>
/// For example, if a field is marked @visible(to: FAMILY) but the user has set their
/// birthday visibility to HIDDEN, the field will not be visible to anyone except the owner.
/// </para>
/// </remarks>
public sealed class VisibleDirectiveType : DirectiveType<VisibleDirective>
{
    /// <inheritdoc />
    protected override void Configure(IDirectiveTypeDescriptor<VisibleDirective> descriptor)
    {
        descriptor.Name("visible");
        descriptor.Description(
            "Controls field visibility based on the viewer's relationship to the profile owner. " +
            "OWNER: only the profile owner can see this field. " +
            "FAMILY: family members can see this field. " +
            "PUBLIC: all authenticated users can see this field.");

        // The directive can be applied to field definitions
        descriptor.Location(DirectiveLocation.FieldDefinition);

        // Argument: 'to' specifies the visibility level
        descriptor
            .Argument(d => d.To)
            .Name("to")
            .Type<NonNullType<EnumType<FieldVisibility>>>()
            .Description("The visibility level for this field.");
    }
}

/// <summary>
/// The runtime representation of the @visible directive.
/// </summary>
public sealed class VisibleDirective
{
    /// <summary>
    /// The visibility level for the field.
    /// </summary>
    public FieldVisibility To { get; set; } = FieldVisibility.Family;
}
