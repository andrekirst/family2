using FamilyDomain = FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using FamilyHub.Modules.Auth.Infrastructure.Extensions;

namespace FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;

/// <summary>
/// Handler for CompleteZitadelLoginCommand.
/// Exchanges authorization code for tokens and creates/syncs user.
/// </summary>
public sealed partial class CompleteZitadelLoginCommandHandler(
    IOptions<ZitadelSettings> settings,
    IUserRepository userRepository,
    IFamilyRepository familyRepository,
    IUnitOfWork unitOfWork,
    IHttpClientFactory httpClientFactory,
    ILogger<CompleteZitadelLoginCommandHandler> logger)
    : IRequestHandler<CompleteZitadelLoginCommand, CompleteZitadelLoginResult>
{
    private readonly ZitadelSettings _settings = settings.Value;
    private readonly JwtSecurityTokenHandler _jwtHandler = new();

    public async Task<CompleteZitadelLoginResult> Handle(
        CompleteZitadelLoginCommand request,
        CancellationToken cancellationToken)
    {
        // TODO Decorator
        logger.LogInformation("Completing Zitadel OAuth login");

        // 1. Exchange authorization code for tokens
        var tokenResponse = await ExchangeAuthorizationCodeAsync(
            request.AuthorizationCode,
            request.CodeVerifier,
            cancellationToken);

        if (tokenResponse.IsError)
        {
            LogFailedToExchangeAuthorizationCodeErrorErrordescription(logger, tokenResponse.Error, tokenResponse.ErrorDescription);
            throw new InvalidOperationException($"Token exchange failed: {tokenResponse.Error}");
        }

        logger.LogInformation("Successfully exchanged authorization code for tokens");

        // 2. Decode ID token to extract user claims
        var idToken = _jwtHandler.ReadJwtToken(tokenResponse.IdentityToken);
        var sub = idToken.Claims.GetTokenValueByClaimType("sub");
        var emailClaim = idToken.Claims.GetTokenValueByClaimType("email");

        LogExtractedUserClaimsSubSubEmailEmail(logger, sub!, emailClaim!);

        // 3. Get or create user (with auto-created family if new user)
        var email = Email.From(emailClaim!);
        var user = await GetOrCreateUserAsync(sub!, email, idToken, cancellationToken);

        LogUserAuthenticatedViaZitadelOauthUseridUseridEmailEmail(logger, user.Id.Value, email.Value);

        // 4. Return result with Zitadel's ID token (JWT with user identity claims)
        // ID token can be validated by backend JWT middleware, access token cannot
        return new CompleteZitadelLoginResult
        {
            UserId = user.Id,
            Email = user.Email,
            FamilyId = user.FamilyId,
            AccessToken = tokenResponse.IdentityToken!,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// Exchanges authorization code for OAuth tokens using PKCE.
    /// </summary>
    private async Task<TokenResponse> ExchangeAuthorizationCodeAsync(
        string authorizationCode,
        string codeVerifier,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();

        var tokenRequest = new AuthorizationCodeTokenRequest
        {
            Address = _settings.TokenEndpoint,
            ClientId = _settings.ClientId,
            ClientSecret = _settings.ClientSecret,
            Code = authorizationCode,
            RedirectUri = _settings.RedirectUri,
            CodeVerifier = codeVerifier // PKCE
        };

        return await httpClient.RequestAuthorizationCodeTokenAsync(tokenRequest, cancellationToken);
    }

    /// <summary>
    /// Gets existing user or creates new user from OAuth provider with personal family.
    /// </summary>
    private async Task<User> GetOrCreateUserAsync(
        string zitadelUserId,
        Email email,
        JwtSecurityToken idToken,
        CancellationToken cancellationToken)
    {
        // Try to find existing user by Zitadel user ID
        var user = await userRepository.GetByExternalUserIdAsync(
            zitadelUserId,
            "zitadel",
            cancellationToken);

        if (user != null)
        {
            LogFoundExistingUserUseridUserid(logger, user.Id.Value);
            return user;
        }

        // Extract display name from ID token for family name
        var displayName = idToken.Claims.GetTokenValueByClaimType("name")
                          ?? idToken.Claims.GetTokenValueByClaimType("preferred_username")
                          ?? email.Value.Split('@')[0];

        var familyName = FamilyName.From($"{displayName} Family");

        // Create personal family first (need ID for user)
        var personalFamily = FamilyDomain.Family.Create(familyName, UserId.New()); // Temporary owner
        await familyRepository.AddAsync(personalFamily, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Create user with family ID
        user = User.CreateFromOAuth(email, zitadelUserId, "zitadel", personalFamily.Id);

        // Transfer ownership to user (now that user has ID)
        personalFamily.TransferOwnership(user.Id);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogCreatedNewUserFromZitadelOauthUseridUseridEmailEmail(logger, user.Id.Value, email.Value);
        LogCreatedPersonalFamilyFamilyidFamilynameForUserUserid(logger, personalFamily.Id.Value, familyName.Value, user.Id.Value);

        return user;
    }

    [LoggerMessage(LogLevel.Error, "Failed to exchange authorization code: {error} - {errorDescription}")]
    static partial void LogFailedToExchangeAuthorizationCodeErrorErrordescription(ILogger<CompleteZitadelLoginCommandHandler> logger, string? error, string? errorDescription);

    [LoggerMessage(LogLevel.Information, "Extracted user claims: sub={sub}, email={email}")]
    static partial void LogExtractedUserClaimsSubSubEmailEmail(ILogger<CompleteZitadelLoginCommandHandler> logger, string sub, string email);

    [LoggerMessage(LogLevel.Information, "User authenticated via Zitadel OAuth: UserId={userId}, Email={email}")]
    static partial void LogUserAuthenticatedViaZitadelOauthUseridUseridEmailEmail(ILogger<CompleteZitadelLoginCommandHandler> logger, Guid userId, string email);

    [LoggerMessage(LogLevel.Information, "Found existing user: UserId={userId}")]
    static partial void LogFoundExistingUserUseridUserid(ILogger<CompleteZitadelLoginCommandHandler> logger, Guid userId);

    [LoggerMessage(LogLevel.Information, "Created new user from Zitadel OAuth: UserId={userId}, Email={email}")]
    static partial void LogCreatedNewUserFromZitadelOauthUseridUseridEmailEmail(ILogger<CompleteZitadelLoginCommandHandler> logger, Guid userId, string email);

    [LoggerMessage(LogLevel.Information, "Created personal family {familyId} '{familyName}' for user {userId}")]
    static partial void LogCreatedPersonalFamilyFamilyidFamilynameForUserUserid(ILogger<CompleteZitadelLoginCommandHandler> logger, Guid familyId, string familyName, Guid userId);
}
