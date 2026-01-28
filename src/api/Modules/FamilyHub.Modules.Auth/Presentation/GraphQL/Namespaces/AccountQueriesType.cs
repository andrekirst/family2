using HotChocolate.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="AccountQueries"/>.
/// </summary>
public sealed class AccountQueriesType : ObjectType<AccountQueries>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<AccountQueries> descriptor)
    {
        descriptor.Name("AccountQueries");
        descriptor.Description("Account-related queries (current user context, profile, settings).");

        // Fields are added via extensions
        descriptor.BindFieldsImplicitly();
    }
}
