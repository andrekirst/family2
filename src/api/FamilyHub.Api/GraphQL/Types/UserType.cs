using FamilyHub.Api.Application.Queries;

namespace FamilyHub.Api.GraphQL;

public class UserType : ObjectType<MeResult>
{
    protected override void Configure(IObjectTypeDescriptor<MeResult> descriptor)
    {
        descriptor.Name("User");

        descriptor.Field(u => u.Id)
            .Type<NonNullType<UuidType>>();

        descriptor.Field(u => u.Email)
            .Type<NonNullType<StringType>>();

        descriptor.Field(u => u.EmailVerified)
            .Type<NonNullType<BooleanType>>();

        descriptor.Field(u => u.EmailVerifiedAt)
            .Type<DateTimeType>();

        descriptor.Field(u => u.CreatedAt)
            .Type<NonNullType<DateTimeType>>();
    }
}
