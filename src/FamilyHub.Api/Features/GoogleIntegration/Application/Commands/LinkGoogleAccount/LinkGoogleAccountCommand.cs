using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;

public sealed record LinkGoogleAccountCommand(
    string Code,
    string State
) : ICommand<LinkGoogleAccountResult>;
