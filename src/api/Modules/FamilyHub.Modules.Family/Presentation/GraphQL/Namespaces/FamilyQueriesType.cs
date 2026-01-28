using HotChocolate.Types;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="FamilyQueries"/>.
/// </summary>
public sealed class FamilyQueriesType : ObjectType<FamilyQueries>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<FamilyQueries> descriptor)
    {
        descriptor.Name("FamilyQueries");
        descriptor.Description("Family-related queries (members, invitations, settings).");

        // Fields are added via extensions
        descriptor.BindFieldsImplicitly();
    }
}
