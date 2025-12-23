using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FluentValidation;
using HotChocolate;
using HotChocolate.Types;
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
    public async Task<CompleteZitadelLoginPayload> CompleteZitadelLogin(
        CompleteZitadelLoginInput input,
        [Service] IMediator mediator,
        [Service] ILogger<AuthMutations> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("GraphQL: completeZitadelLogin mutation called");

            // Create command from input
            var command = new CompleteZitadelLoginCommand(
                AuthorizationCode: input.AuthorizationCode,
                CodeVerifier: input.CodeVerifier);

            // Send command via MediatR (automatic validation via ValidationBehavior)
            var result = await mediator.Send(command, cancellationToken);

            // Map to GraphQL type
            var userType = new UserType
            {
                Id = result.UserId.Value,
                Email = result.Email.Value,
                EmailVerified = result.EmailVerified,
                CreatedAt = DateTime.UtcNow // TODO: Get from user entity when needed
            };

            var authenticationResult = new AuthenticationResult
            {
                User = userType,
                AccessToken = result.AccessToken,
                RefreshToken = null, // No refresh tokens with Zitadel (use their refresh flow)
                ExpiresAt = result.ExpiresAt
            };

            logger.LogInformation(
                "User logged in successfully via Zitadel OAuth: {UserId}",
                result.UserId.Value);

            return CompleteZitadelLoginPayload.Success(authenticationResult);
        }
        catch (ValidationException ex)
        {
            // FluentValidation errors
            logger.LogWarning("Zitadel login failed: Validation errors");

            // TODO Create generic handler or middleware to handle error generic
            var errors = ex.Errors.Select(error => new UserError
            {
                Message = error.ErrorMessage,
                Code = "VALIDATION_ERROR",
                Field = error.PropertyName
            }).ToArray();

            return CompleteZitadelLoginPayload.Failure(errors);
        }
        catch (InvalidOperationException ex)
        {
            // OAuth token exchange errors
            logger.LogError(ex, "Zitadel login failed: Token exchange error");

            return CompleteZitadelLoginPayload.Failure(new UserError
            {
                Message = "Failed to complete OAuth login. Please try again.",
                Code = "OAUTH_ERROR",
                Field = null
            });
        }
        catch (Exception ex)
        {
            // Unexpected errors
            logger.LogError(ex, "Unexpected error during Zitadel login");

            return CompleteZitadelLoginPayload.Failure(new UserError
            {
                Message = "An unexpected error occurred during login. Please try again.",
                Code = "INTERNAL_ERROR",
                Field = null
            });
        }
    }
}
