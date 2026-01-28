using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using HotChocolate;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL object type for Family entity.
/// Implements the Relay Node interface for global ID resolution.
/// </summary>
/// <remarks>
/// <para>
/// This type:
/// <list type="bullet">
/// <item><description>Exposes Family entity as a GraphQL "Family" type</description></item>
/// <item><description>Implements Node interface with base64-encoded global IDs</description></item>
/// <item><description>Provides node resolution via repository lookup</description></item>
/// </list>
/// </para>
/// <para>
/// The global ID format is: base64("Family:{guid}")
/// </para>
/// </remarks>
public class FamilyType : ObjectType<FamilyAggregate>
{
    /// <summary>
    /// Configures the GraphQL type descriptor for the Family entity.
    /// </summary>
    /// <param name="descriptor">The object type descriptor to configure.</param>
    protected override void Configure(IObjectTypeDescriptor<FamilyAggregate> descriptor)
    {
        descriptor.Name("Family");
        descriptor.Description("Represents a family in the system.");

        descriptor.BindFieldsExplicitly();

        // Implement Relay Node interface
        descriptor
            .ImplementsNode()
            .IdField(f => f.Id.Value)
            .ResolveNode(async (ctx, id) =>
            {
                var repository = ctx.Service<IFamilyRepository>();
                return await repository.GetByIdAsync(FamilyId.From(id), ctx.RequestAborted);
            });

        // Override the ID field to return global ID
        descriptor
            .Field("id")
            .Type<NonNullType<IdType>>()
            .Description("Global ID (Relay Node specification)")
            .Resolve(ctx => GlobalIdSerializer.Serialize("Family", ctx.Parent<FamilyAggregate>().Id.Value));

        // Raw internal ID for backward compatibility
        descriptor
            .Field("internalId")
            .Type<NonNullType<UuidType>>()
            .Description("Internal UUID. Prefer using 'id' (global ID) for client operations.")
            .Resolve(ctx => ctx.Parent<FamilyAggregate>().Id.Value);

        // Family name
        descriptor
            .Field(f => f.Name)
            .Name("name")
            .Type<NonNullType<StringType>>()
            .Description("Family name")
            .Resolve(ctx => ctx.Parent<FamilyAggregate>().Name.Value);

        // Owner ID as global ID
        descriptor
            .Field("ownerId")
            .Type<NonNullType<IdType>>()
            .Description("Global ID of the family owner")
            .Resolve(ctx => GlobalIdSerializer.Serialize("User", ctx.Parent<FamilyAggregate>().OwnerId.Value));

        // Audit info
        descriptor
            .Field("auditInfo")
            .Type<NonNullType<ObjectType<AuditInfoType>>>()
            .Description("Audit metadata (creation and last update timestamps)")
            .Resolve(ctx =>
            {
                var family = ctx.Parent<FamilyAggregate>();
                return new AuditInfoType
                {
                    CreatedAt = family.CreatedAt,
                    UpdatedAt = family.UpdatedAt
                };
            });

        // NOTE: Members and owner fields are added via FamilyTypeExtensions in Auth module
        // (temporarily, until Phase 5+ when User aggregate is properly abstracted).
    }
}
