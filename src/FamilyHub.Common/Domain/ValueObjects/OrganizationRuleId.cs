using Vogen;

namespace FamilyHub.Common.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct OrganizationRuleId
{
    public static OrganizationRuleId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty
            ? Validation.Invalid("OrganizationRuleId cannot be empty")
            : Validation.Ok;
}
