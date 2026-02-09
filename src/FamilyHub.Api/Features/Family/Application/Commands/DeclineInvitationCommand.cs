using FamilyHub.Api.Common.Application;

namespace FamilyHub.Api.Features.Family.Application.Commands;

/// <summary>
/// Command to decline a family invitation using the plaintext token.
/// </summary>
public sealed record DeclineInvitationCommand(
    string Token
) : ICommand<bool>;
