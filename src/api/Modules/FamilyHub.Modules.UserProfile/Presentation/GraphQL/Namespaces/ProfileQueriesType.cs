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

        // Fields are added via extensions
        descriptor.BindFieldsImplicitly();
    }
}
