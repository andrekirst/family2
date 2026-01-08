using FamilyHub.Infrastructure.GraphQL;
using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
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

    [UseMutationConvention]
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    [Error(typeof(ValueObjectError))]
    [Error(typeof(UnauthorizedError))]
    [Error(typeof(InternalServerError))]
    public async Task<AuthenticationResult> CompleteZitadelLogin(
        CompleteZitadelLoginInput input,
        [Service] IMediator mediator,
        [Service] ILogger<AuthMutations> logger,
        CancellationToken cancellationToken)
    {
        LogCompleteZitadelLoginMutationCalled(logger, null);

        // Map input to command
        var command = new CompleteZitadelLoginCommand(
            AuthorizationCode: input.AuthorizationCode,
            CodeVerifier: input.CodeVerifier);

        // Send command via MediatR (automatic validation via ValidationBehavior)
        var result = await mediator.Send(command, cancellationToken);

        // Map result → return data directly
        var authResult = new AuthenticationResult
        {
            User = new UserType
            {
                Id = result.UserId.Value,
                Email = result.Email.Value,
                EmailVerified = result.EmailVerified,
                AuditInfo = result.AsAuditInfo()
            },
            AccessToken = result.AccessToken,
            RefreshToken = null, // OAuth providers like Zitadel don't use refresh tokens
            ExpiresAt = result.ExpiresAt
        };

        // Log successful login
        LogUserLoggedIn(logger, authResult.User.Id, null);

        return authResult;
    }
}
