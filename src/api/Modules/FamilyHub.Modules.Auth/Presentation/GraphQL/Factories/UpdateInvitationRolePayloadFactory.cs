using FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Factories;

/// <summary>
/// Factory for creating UpdateInvitationRolePayload instances.
/// </summary>
public class UpdateInvitationRolePayloadFactory
    : IPayloadFactory<UpdateInvitationRoleResult, UpdateInvitationRolePayload>
{
    public UpdateInvitationRolePayload Success(UpdateInvitationRoleResult result)
    {
        var invitation = new PendingInvitationType
        {
            Id = result.InvitationId.Value,
            Email = string.Empty, // Not available in result
            Role = MapToGraphQLRole(result.Role),
            Status = InvitationStatusType.PENDING, // Not changed by update
            InvitedById = Guid.Empty, // Not available in result
            InvitedAt = DateTime.UtcNow, // Not available in result
            ExpiresAt = DateTime.UtcNow.AddDays(14) // Not available in result
        };

        return new UpdateInvitationRolePayload(invitation);
    }

    public UpdateInvitationRolePayload Error(IReadOnlyList<UserError> errors)
    {
        return new UpdateInvitationRolePayload(errors);
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
}
