using HotChocolate.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="AccountMutations"/>.
/// </summary>
public sealed class AccountMutationsType : ObjectType<AccountMutations>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<AccountMutations> descriptor)
    {
        descriptor.Name("AccountMutations");
        descriptor.Description("Account-related mutations (profile updates, settings, invitation acceptance).");

        // Fields are added via extensions
        descriptor.BindFieldsImplicitly();
    }
}
