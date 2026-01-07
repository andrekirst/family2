using FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Factories;

/// <summary>
/// Factory for creating AcceptInvitationPayload instances.
/// Maps the command result to GraphQL payload with family and role information.
/// </summary>
public class AcceptInvitationPayloadFactory(IFamilyRepository familyRepository)
    : IPayloadFactory<AcceptInvitationResult, AcceptInvitationPayload>
{
    public AcceptInvitationPayload Success(AcceptInvitationResult result)
    {
        // Fetch the family entity to return in the payload
        // Note: This is a synchronous call in the factory, which is not ideal
        // but follows the current factory pattern for consistency
        var family = familyRepository.GetByIdAsync(result.FamilyId).GetAwaiter().GetResult();

        if (family == null)
        {
            throw new InvalidOperationException($"Family {result.FamilyId.Value} not found");
        }

        // Map domain role to GraphQL role enum
        var roleType = result.Role.Value.ToLowerInvariant() switch
        {
            "owner" => UserRoleType.OWNER,
            "admin" => UserRoleType.ADMIN,
            "member" => UserRoleType.MEMBER,
            _ => throw new InvalidOperationException($"Unknown role: {result.Role.Value}")
        };

        return new AcceptInvitationPayload(family, roleType);
    }

    public AcceptInvitationPayload Error(IReadOnlyList<UserError> errors)
    {
        return new AcceptInvitationPayload(errors);
    }
}
