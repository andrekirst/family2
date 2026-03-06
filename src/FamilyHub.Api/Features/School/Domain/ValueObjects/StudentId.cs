using Vogen;

namespace FamilyHub.Api.Features.School.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct StudentId
{
    public static StudentId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("Student ID cannot be empty");
        }

        return Validation.Ok;
    }
}
