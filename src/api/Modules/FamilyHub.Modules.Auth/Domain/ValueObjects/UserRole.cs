namespace FamilyHub.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Represents a user's role within a family.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct UserRole
{
    private static readonly string[] ValidRoles = ["owner", "admin", "member", "child"];

    public static readonly UserRole Owner = From("owner");
    public static readonly UserRole Admin = From("admin");
    public static readonly UserRole Member = From("member");
    public static readonly UserRole Child = From("child");

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("User role cannot be empty.");
        }

        if (!ValidRoles.Contains(value.ToLowerInvariant()))
        {
            return Validation.Invalid($"Invalid user role. Must be one of: {string.Join(", ", ValidRoles)}");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
