using Vogen;

namespace FamilyHub.Api.Features.Family.Domain.ValueObjects;

/// <summary>
/// Family member role value object.
/// Valid roles: "Owner", "Admin", "Member".
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyRole
{
    public static FamilyRole Owner => From("Owner");
    public static FamilyRole Admin => From("Admin");
    public static FamilyRole Member => From("Member");

    private static readonly string[] ValidRoles = ["Owner", "Admin", "Member"];

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Family role is required");
        }

        if (!ValidRoles.Contains(value))
        {
            return Validation.Invalid($"Invalid family role: '{value}'. Valid roles are: {string.Join(", ", ValidRoles)}");
        }

        return Validation.Ok;
    }

    public bool CanInvite() => Value is "Owner" or "Admin";
}
