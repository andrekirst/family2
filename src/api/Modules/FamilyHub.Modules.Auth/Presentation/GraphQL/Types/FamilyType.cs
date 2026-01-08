using HotChocolate.Types;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL object type for Family entity.
/// Uses HotChocolate ObjectType pattern to configure domain entity mapping.
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

        descriptor
            .Field("members")
            .Type<NonNullType<ListType<NonNullType<ObjectType<FamilyMemberType>>>>>()
            .Description("Active members of this family with their roles")
            .Resolve(ctx =>
            {
                var family = ctx.Parent<Family>();
                // Map domain Users to FamilyMemberType with roles
                return family.Members
                    .Where(u => u.DeletedAt == null) // Exclude soft-deleted users
                    .Select(user => new FamilyMemberType
                    {
                        Id = user.Id.Value,
                        Email = user.Email.Value,
                        EmailVerified = user.EmailVerified,
                        Role = MapToGraphQLRole(user.GetRoleInFamily(family)),
                        JoinedAt = user.CreatedAt, // User creation time = family join time
                        IsOwner = user.Id == family.OwnerId,
                        AuditInfo = new AuditInfoType
                        {
                            CreatedAt = user.CreatedAt,
                            UpdatedAt = user.UpdatedAt
                        }
                    })
                    .ToList();
            });
    }

    /// <summary>
    /// Maps domain FamilyRole to GraphQL UserRoleType.
    /// </summary>
    private static UserRoleType MapToGraphQLRole(FamilyRole domainRole)
    {
        var roleValue = domainRole.Value.ToLowerInvariant();
        return roleValue switch
        {
            "owner" => UserRoleType.OWNER,
            "admin" => UserRoleType.ADMIN,
            "member" => UserRoleType.MEMBER,
            _ => throw new InvalidOperationException($"Unknown role: {roleValue}")
        };
    }
}
