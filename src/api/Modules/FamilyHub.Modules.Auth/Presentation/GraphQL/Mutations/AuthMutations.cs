using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Presentation.GraphQL;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for authentication operations.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class AuthMutations
{
    /// <summary>
    /// Completes Zitadel OAuth login by exchanging authorization code for tokens.
    /// </summary>
    // High-performance logging delegates using LoggerMessage.Define (CA1873 compliant)
    private static readonly Action<ILogger, Exception?> LogCompleteZitadelLoginMutationCalled =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3100, nameof(LogCompleteZitadelLoginMutationCalled)),
            "GraphQL: completeZitadelLogin mutation called");

    private static readonly Action<ILogger, Guid, Exception?> LogUserLoggedIn =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(3101, nameof(LogUserLoggedIn)),
            "User logged in successfully via Zitadel OAuth: {UserId}");

    public async Task<CompleteZitadelLoginPayload> CompleteZitadelLogin(
        CompleteZitadelLoginInput input,
        [Service] IMutationHandler mutationHandler,
        [Service] IMediator mediator,
        [Service] ILogger<AuthMutations> logger,
        CancellationToken cancellationToken)
    {
        LogCompleteZitadelLoginMutationCalled(logger, null);

        var payload = await mutationHandler.Handle<CompleteZitadelLoginResult, CompleteZitadelLoginPayload>(async () =>
        {
            // Map input to command
            var command = new CompleteZitadelLoginCommand(
                AuthorizationCode: input.AuthorizationCode,
                CodeVerifier: input.CodeVerifier);

            // Send command via MediatR (automatic validation via ValidationBehavior)
            return await mediator.Send(command, cancellationToken);
        });

        // Log successful login if no errors
        if (payload.Success && payload.AuthenticationResult != null)
        {
            LogUserLoggedIn(logger, payload.AuthenticationResult.User.Id, null);
        }

        return payload;
    }
}
