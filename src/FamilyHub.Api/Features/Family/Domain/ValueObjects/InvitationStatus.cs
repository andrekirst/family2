using Vogen;

namespace FamilyHub.Api.Features.Family.Domain.ValueObjects;

/// <summary>
/// Invitation status value object.
/// Valid statuses: "Pending", "Accepted", "Declined", "Revoked", "Expired".
/// </summary>
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationStatus
{
    public static InvitationStatus Pending => From("Pending");
    public static InvitationStatus Accepted => From("Accepted");
    public static InvitationStatus Declined => From("Declined");
    public static InvitationStatus Revoked => From("Revoked");
    public static InvitationStatus Expired => From("Expired");

    private static readonly string[] ValidStatuses = ["Pending", "Accepted", "Declined", "Revoked", "Expired"];

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Invitation status is required");
        }

        if (!ValidStatuses.Contains(value))
        {
            return Validation.Invalid($"Invalid invitation status: '{value}'");
        }

        return Validation.Ok;
    }

    public bool IsPending() => Value == "Pending";
    public bool IsTerminal() => Value is "Accepted" or "Declined" or "Revoked" or "Expired";
}
