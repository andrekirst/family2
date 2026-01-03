using HotChocolate.Types;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Infrastructure.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL object type for Family entity
/// </summary>
public class FamilyType : ObjectType<Family>
{
    protected override void Configure(IObjectTypeDescriptor<Family> descriptor)
    {
        descriptor.Name("Family");
        descriptor.Description("Represents a family in the system");

        descriptor.BindFieldsExplicitly();

        descriptor
            .Field(f => f.Id)
            .Name("id")
            .Type<NonNullType<UuidType>>()
            .Description("Unique family identifier")
            .Resolve(ctx => ctx.Parent<Family>().Id.Value);

        descriptor
            .Field(f => f.Name)
            .Name("name")
            .Type<NonNullType<StringType>>()
            .Description("Family name")
            .Resolve(ctx => ctx.Parent<Family>().Name.Value);

        descriptor
            .Field(f => f.OwnerId)
            .Name("ownerId")
            .Type<NonNullType<UuidType>>()
            .Description("User ID of the family owner")
            .Resolve(ctx => ctx.Parent<Family>().OwnerId.Value);

        descriptor
            .Field("auditInfo")
            .Type<NonNullType<ObjectType<AuditInfoType>>>()
            .Description("Audit metadata (creation and last update timestamps)")
            .Resolve(ctx =>
            {
                var family = ctx.Parent<Family>();
                return new AuditInfoType
                {
                    CreatedAt = family.CreatedAt,
                    UpdatedAt = family.UpdatedAt
                };
            });

    }
}
