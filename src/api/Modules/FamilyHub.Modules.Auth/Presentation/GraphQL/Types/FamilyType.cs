using HotChocolate.Types;
using FamilyHub.Modules.Auth.Domain;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL object type for Family entity.
/// Maps domain entity to GraphQL schema with automatic projections.
/// </summary>
public class FamilyType : ObjectType<Family>
{
    protected override void Configure(IObjectTypeDescriptor<Family> descriptor)
    {
        descriptor.Name("Family");
        descriptor.Description("Represents a family in the system");

        // Map FamilyId value object to scalar
        descriptor
            .Field(f => f.Id)
            .Type<NonNullType<UuidType>>()
            .Resolve(ctx => ctx.Parent<Family>().Id.Value);

        // Map FamilyName value object to string
        descriptor
            .Field(f => f.Name)
            .Type<NonNullType<StringType>>()
            .Resolve(ctx => ctx.Parent<Family>().Name.Value);

        // Map UserId value object to scalar
        descriptor
            .Field(f => f.OwnerId)
            .Type<NonNullType<UuidType>>()
            .Resolve(ctx => ctx.Parent<Family>().OwnerId.Value);

        // Direct field mappings
        descriptor.Field(f => f.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(f => f.UpdatedAt).Type<NonNullType<DateTimeType>>();

        // Ignore internal fields
        descriptor.Ignore(f => f.DeletedAt);
        descriptor.Ignore(f => f.UserFamilies);
    }
}
