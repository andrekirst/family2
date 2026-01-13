using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;

/// <summary>
/// Command to accept a family invitation using a token.
/// Validates the invitation and adds the authenticated user to the family.
/// SPECIAL CASE: Requires authentication but NOT IRequireFamilyContext,
/// because the user is joining a family and doesn't have one yet.
/// </summary>
public record AcceptInvitationCommand(
    InvitationToken Token
) : IRequest<FamilyHub.SharedKernel.Domain.Result<AcceptInvitationResult>>,
    IRequireAuthentication;
