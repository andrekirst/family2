using MediatR;

namespace FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;

/// <summary>
/// Command to complete Zitadel OAuth login by exchanging authorization code for tokens.
/// </summary>
public sealed record CompleteZitadelLoginCommand(
    string AuthorizationCode,
    string CodeVerifier
) : IRequest<CompleteZitadelLoginResult>;
