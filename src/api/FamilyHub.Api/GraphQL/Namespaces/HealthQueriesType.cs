using HotChocolate.Types;

namespace FamilyHub.Api.GraphQL.Namespaces;

/// <summary>
/// ObjectType configuration for <see cref="HealthQueries"/>.
/// </summary>
public sealed class HealthQueriesType : ObjectType<HealthQueries>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<HealthQueries> descriptor)
    {
        descriptor.Name("HealthQueries");
        descriptor.Description(
            "Health check queries. Liveness is public, details require authentication.");

        // Fields are added via HealthQueriesExtensions
        descriptor.BindFieldsImplicitly();
    }
}
