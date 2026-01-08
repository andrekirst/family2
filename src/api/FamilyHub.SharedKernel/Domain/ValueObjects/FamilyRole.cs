namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a user's role within a family.
/// This value object is shared across modules to prevent circular dependencies.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyRole
{
    private static readonly string[] ValidRoles =
    [
        "owner",
        "admin",
        "member",
        "child"
    ];

    /// <summary>
    /// Owner role - full family administration permissions.
    /// Can transfer ownership, remove members, delete family.
    /// </summary>
    public static readonly FamilyRole Owner = From("owner");

    /// <summary>
    /// Admin role - family management permissions.
    /// Can manage members, but cannot transfer ownership.
    /// </summary>
    public static readonly FamilyRole Admin = From("admin");

    /// <summary>
    /// Member role - standard family member permissions.
    /// Can view family data, manage personal tasks and calendars.
    /// </summary>
    public static readonly FamilyRole Member = From("member");

    /// <summary>
    /// Child role - limited permissions for child accounts.
    /// Managed by family admins with restricted capabilities.
    /// </summary>
    public static readonly FamilyRole Child = From("child");

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Family role cannot be empty.");
        }

        if (!ValidRoles.Contains(value.ToLowerInvariant()))
        {
            return Validation.Invalid($"Invalid family role. Must be one of: {string.Join(", ", ValidRoles)}");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
