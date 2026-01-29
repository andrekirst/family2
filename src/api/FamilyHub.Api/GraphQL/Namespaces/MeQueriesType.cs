using HotChocolate.Types;

namespace FamilyHub.Api.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="MeQueries"/>.
/// </summary>
public sealed class MeQueriesType : ObjectType<MeQueries>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<MeQueries> descriptor)
    {
        descriptor.Name("MeQueries");
        descriptor.Description(
            "User-centric queries for the authenticated user. " +
            "Access profile, family, and pending invitations.");

        // Fields are added via MeQueriesExtensions
        descriptor.BindFieldsImplicitly();
    }
}
