using Vogen;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct GoogleLinkStatus
{
    public static GoogleLinkStatus Active => From("Active");
    public static GoogleLinkStatus Revoked => From("Revoked");
    public static GoogleLinkStatus Expired => From("Expired");
    public static GoogleLinkStatus Error => From("Error");

    private static readonly string[] ValidStatuses = ["Active", "Revoked", "Expired", "Error"];

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Google link status cannot be empty");
        if (!ValidStatuses.Contains(value))
            return Validation.Invalid($"Invalid status '{value}'. Valid values: {string.Join(", ", ValidStatuses)}");
        return Validation.Ok;
    }

    public bool IsActive => Value == "Active";
}
