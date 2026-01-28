using HotChocolate.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="AuthMutations"/>.
/// </summary>
public sealed class AuthMutationsType : ObjectType<AuthMutations>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<AuthMutations> descriptor)
    {
        descriptor.Name("AuthMutations");
        descriptor.Description("Authentication-related mutations (login, register, password management).");

        // Fields are added via extensions
        descriptor.BindFieldsImplicitly();
    }
}
