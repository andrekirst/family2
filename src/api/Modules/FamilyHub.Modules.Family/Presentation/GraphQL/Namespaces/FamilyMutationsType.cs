using HotChocolate.Types;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="FamilyMutations"/>.
/// </summary>
public sealed class FamilyMutationsType : ObjectType<FamilyMutations>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<FamilyMutations> descriptor)
    {
        descriptor.Name("FamilyMutations");
        descriptor.Description("Family-related mutations (create, invite, manage members).");

        // Fields are added via extensions
        descriptor.BindFieldsImplicitly();
    }
}
