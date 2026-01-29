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

        // Placeholder field - actual queries to be added via extensions
        // NOTE: GraphQL object types require at least one field
        descriptor
            .Field("_placeholder")
            .Type<NonNullType<StringType>>()
            .Description("Placeholder field. Authentication queries coming soon.")
            .Resolve(_ => "Auth queries namespace - queries to be implemented");
    }
}
