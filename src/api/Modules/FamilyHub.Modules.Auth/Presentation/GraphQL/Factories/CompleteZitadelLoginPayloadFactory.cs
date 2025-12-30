using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Factories;

/// <summary>
/// Factory for creating CompleteZitadelLoginPayload instances.
/// Implements the IPayloadFactory pattern for type-safe, reflection-free payload construction.
/// </summary>
public class CompleteZitadelLoginPayloadFactory : IPayloadFactory<CompleteZitadelLoginResult, CompleteZitadelLoginPayload>
{
    /// <summary>
    /// Creates a success payload from the CompleteZitadelLoginResult.
    /// </summary>
    /// <param name="result">The successful CompleteZitadelLoginResult from the command handler</param>
    /// <returns>A CompleteZitadelLoginPayload containing the authentication result</returns>
    public CompleteZitadelLoginPayload Success(CompleteZitadelLoginResult result)
    {
        // Map the command result to GraphQL types
        var userType = new UserType
        {
            Id = result.UserId.Value,
            Email = result.Email.Value,
            EmailVerified = result.EmailVerified,
            CreatedAt = result.CreatedAt
        };

        var authenticationResult = new AuthenticationResult
        {
            User = userType,
            AccessToken = result.AccessToken,
            RefreshToken = null, // TODO: Add refresh token support
            ExpiresAt = result.ExpiresAt
        };

        return new CompleteZitadelLoginPayload(authenticationResult);
    }

    /// <summary>
    /// Creates an error payload from a list of errors.
    /// </summary>
    /// <param name="errors">List of errors that occurred during login</param>
    /// <returns>A CompleteZitadelLoginPayload containing the errors</returns>
    public CompleteZitadelLoginPayload Error(IReadOnlyList<UserError> errors)
    {
        return new CompleteZitadelLoginPayload(errors);
    }
}
