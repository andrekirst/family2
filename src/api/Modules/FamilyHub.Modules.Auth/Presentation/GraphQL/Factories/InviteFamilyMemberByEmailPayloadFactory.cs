using FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Factories;

/// <summary>
/// Factory for creating InviteFamilyMemberByEmailPayload instances.
/// </summary>
public class InviteFamilyMemberByEmailPayloadFactory
    : IPayloadFactory<InviteFamilyMemberByEmailResult, InviteFamilyMemberByEmailPayload>
{
    public InviteFamilyMemberByEmailPayload Success(InviteFamilyMemberByEmailResult result)
    {
        var invitation = new PendingInvitationType
        {
            Id = result.InvitationId.Value,
            Email = result.Email.Value,
            Role = MapToGraphQLRole(result.Role),
            Status = MapToGraphQLStatus(result.Status),
            InvitedAt = result.ExpiresAt.AddDays(-14), // Calculated from ExpiresAt
            ExpiresAt = result.ExpiresAt,
            DisplayCode = result.DisplayCode.Value
        };

        return new InviteFamilyMemberByEmailPayload(invitation);
    }

    public InviteFamilyMemberByEmailPayload Error(IReadOnlyList<UserError> errors)
    {
        return new InviteFamilyMemberByEmailPayload(errors);
    }

    /// <summary>
    /// Maps domain UserRole to GraphQL UserRoleType.
    /// </summary>
    private static UserRoleType MapToGraphQLRole(Domain.ValueObjects.UserRole domainRole)
    {
        return domainRole.Value.ToLowerInvariant() switch
        {
            "owner" => UserRoleType.OWNER,
            "admin" => UserRoleType.ADMIN,
            "member" => UserRoleType.MEMBER,
            _ => throw new InvalidOperationException($"Unknown role: {domainRole.Value}")
        };
    }

    /// <summary>
    /// Maps domain InvitationStatus to GraphQL InvitationStatusType.
    /// </summary>
    private static InvitationStatusType MapToGraphQLStatus(Domain.ValueObjects.InvitationStatus domainStatus)
    {
        return domainStatus.Value.ToLowerInvariant() switch
        {
            "pending" => InvitationStatusType.PENDING,
            "accepted" => InvitationStatusType.ACCEPTED,
            "rejected" => InvitationStatusType.REJECTED,
            "canceled" => InvitationStatusType.CANCELLED,
            "expired" => InvitationStatusType.EXPIRED,
            _ => throw new InvalidOperationException($"Unknown invitation status: {domainStatus.Value}")
        };
    }
}
