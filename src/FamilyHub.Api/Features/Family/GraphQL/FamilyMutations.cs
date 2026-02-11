using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Family.Application.Commands;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// GraphQL mutations for family management operations.
/// Uses Input → Command pattern per ADR-003.
/// Extends AuthMutations (the root mutation type).
/// </summary>
[ExtendObjectType(typeof(AuthMutations))]
public class FamilyMutations
{
    /// <summary>
    /// Create a new family with the current user as owner.
    /// </summary>
    [Authorize]
    public async Task<FamilyDto> CreateFamily(
        CreateFamilyRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IFamilyRepository familyRepository,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        // Get current user from JWT
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        // Convert primitive input to value object (Input → Command pattern)
        var familyName = FamilyName.From(input.Name.Trim());

        // Create command
        var command = new CreateFamilyCommand(familyName, user.Id);

        // Send command via Wolverine
        var result = await commandBus.SendAsync<CreateFamilyResult>(command, ct);

        // Query the created family and map to DTO
        var createdFamily = await familyRepository.GetByIdWithMembersAsync(result.FamilyId, ct);
        if (createdFamily is null)
        {
            throw new InvalidOperationException("Family creation failed");
        }

        return FamilyMapper.ToDto(createdFamily);
    }
}
