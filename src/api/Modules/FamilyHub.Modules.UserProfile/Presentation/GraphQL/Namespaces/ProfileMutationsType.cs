using HotChocolate.Types;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="ProfileMutations"/>.
/// </summary>
public sealed class ProfileMutationsType : ObjectType<ProfileMutations>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<ProfileMutations> descriptor)
    {
        descriptor.Name("ProfileMutations");
        descriptor.Description("Profile-related mutations (update profile, approve/reject changes).");

        // Fields are added via extensions
        descriptor.BindFieldsImplicitly();
    }
}
