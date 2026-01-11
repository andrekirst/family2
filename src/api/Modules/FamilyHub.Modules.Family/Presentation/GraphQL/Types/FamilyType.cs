using FamilyHub.Infrastructure.GraphQL.Types;
using HotChocolate.Types;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL object type for Family entity.
/// Uses HotChocolate ObjectType pattern to configure domain entity mapping.
/// PHASE 4: Extracted from Auth module to Family module.
/// </summary>
public class FamilyType : ObjectType<FamilyAggregate>
{
    /// <summary>
    /// Configures the GraphQL type descriptor for the Family entity.
    /// </summary>
    /// <param name="descriptor">The object type descriptor to configure.</param>
    protected override void Configure(IObjectTypeDescriptor<FamilyAggregate> descriptor)
    {
        descriptor.Name("Family");
        descriptor.Description("Represents a family in the system");

        descriptor.BindFieldsExplicitly();

        descriptor
            .Field(f => f.Id)
            .Name("id")
            .Type<NonNullType<UuidType>>()
            .Description("Unique family identifier")
            .Resolve(ctx => ctx.Parent<FamilyAggregate>().Id.Value);

        descriptor
            .Field(f => f.Name)
            .Name("name")
            .Type<NonNullType<StringType>>()
            .Description("Family name")
            .Resolve(ctx => ctx.Parent<FamilyAggregate>().Name.Value);

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
