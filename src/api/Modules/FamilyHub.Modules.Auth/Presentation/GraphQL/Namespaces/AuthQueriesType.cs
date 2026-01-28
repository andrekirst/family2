using HotChocolate.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="AuthQueries"/>.
/// </summary>
public sealed class AuthQueriesType : ObjectType<AuthQueries>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<AuthQueries> descriptor)
    {
        descriptor.Name("AuthQueries");
        descriptor.Description("Authentication-related queries.");

        // Fields are added via extensions
        descriptor.BindFieldsImplicitly();
    }
}
