using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
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
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        var familyName = FamilyName.From(input.Name.Trim());
        var command = new CreateFamilyCommand(familyName, user.Id);
        var result = await commandBus.SendAsync<CreateFamilyResult>(command, ct);

        var createdFamily = await familyRepository.GetByIdWithMembersAsync(result.FamilyId, ct);
        if (createdFamily is null)
        {
            throw new InvalidOperationException("Family creation failed");
        }

        return FamilyMapper.ToDto(createdFamily);
    }

    /// <summary>
    /// Send a family invitation to an email address.
    /// Requires Owner or Admin role in the family.
    /// </summary>
    [Authorize]
    public async Task<InvitationDto> SendInvitation(
        SendInvitationRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IFamilyInvitationRepository invitationRepository,
        CancellationToken ct)
    {
        var user = await GetCurrentUser(claimsPrincipal, userRepository, ct);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to send invitations");
        }

        var command = new SendInvitationCommand(
            user.FamilyId.Value,
            user.Id,
            Email.From(input.Email.Trim()),
            FamilyRole.From(input.Role));

        var result = await commandBus.SendAsync<SendInvitationResult>(command, ct);

        var invitation = await invitationRepository.GetByIdAsync(result.InvitationId, ct);
        return InvitationMapper.ToDto(invitation!);
    }

    /// <summary>
    /// Accept a family invitation using the token from the email link.
    /// </summary>
    [Authorize]
    public async Task<AcceptInvitationResultDto> AcceptInvitation(
        AcceptInvitationRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await GetCurrentUser(claimsPrincipal, userRepository, ct);

        var command = new AcceptInvitationCommand(input.Token, user.Id);
        var result = await commandBus.SendAsync<AcceptInvitationResult>(command, ct);

        return new AcceptInvitationResultDto
        {
            FamilyId = result.FamilyId.Value,
            FamilyMemberId = result.FamilyMemberId.Value,
            Success = true
        };
    }

    /// <summary>
    /// Decline a family invitation using the token from the email link.
    /// </summary>
    [Authorize]
    public async Task<bool> DeclineInvitation(
        AcceptInvitationRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        // Verify the user is authenticated (even though we don't need user data for decline)
        await GetCurrentUser(claimsPrincipal, userRepository, ct);

        var command = new DeclineInvitationCommand(input.Token);
        return await commandBus.SendAsync<bool>(command, ct);
    }

    /// <summary>
    /// Revoke a pending family invitation (Owner/Admin only).
    /// </summary>
    [Authorize]
    public async Task<bool> RevokeInvitation(
        Guid invitationId,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await GetCurrentUser(claimsPrincipal, userRepository, ct);

        var command = new RevokeInvitationCommand(InvitationId.From(invitationId), user.Id);
        return await commandBus.SendAsync<bool>(command, ct);
    }

    private static async Task<Auth.Domain.Entities.User> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        return await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");
    }
}
