using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;

/// <summary>
/// Command to accept a family invitation using a token.
/// Validates the invitation and adds the authenticated user to the family.
/// </summary>
public record AcceptInvitationCommand(
    InvitationToken Token
) : IRequest<FamilyHub.SharedKernel.Domain.Result<AcceptInvitationResult>>;
