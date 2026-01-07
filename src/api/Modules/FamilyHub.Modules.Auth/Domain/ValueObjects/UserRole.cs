using FamilyHub.Modules.Auth.Domain.Constants;

namespace FamilyHub.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Represents a user's role within a family.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct UserRole
{
    private static readonly string[] ValidRoles =
    [
        UserRoleConstants.OwnerValue,
        UserRoleConstants.AdminValue,
        UserRoleConstants.MemberValue
    ];

    public static readonly UserRole Owner = From(UserRoleConstants.OwnerValue);
    public static readonly UserRole Admin = From(UserRoleConstants.AdminValue);
    public static readonly UserRole Member = From(UserRoleConstants.MemberValue);

    private static Validation Validate(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("User role cannot be empty.")
            : !ValidRoles.Contains(value.ToLowerInvariant())
                ? Validation.Invalid($"Invalid user role. Must be one of: {string.Join(", ", ValidRoles)}")
                : Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
