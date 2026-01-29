using HotChocolate.Types;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="ProfileQueries"/>.
/// </summary>
public sealed class ProfileQueriesType : ObjectType<ProfileQueries>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<ProfileQueries> descriptor)
    {
        descriptor.Name("ProfileQueries");
        descriptor.Description("Profile-related queries (my profile, user profiles, change requests).");

        // Placeholder field - actual queries to be added via extensions
        // NOTE: GraphQL object types require at least one field
        descriptor
            .Field("_placeholder")
            .Type<NonNullType<StringType>>()
            .Description("Placeholder field. Profile queries coming soon.")
            .Resolve(_ => "Profile queries namespace - queries to be implemented");
    }
}
