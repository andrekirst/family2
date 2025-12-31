using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using IdentityModel.Client;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;

/// <summary>
/// Handler for CompleteZitadelLoginCommand.
/// Exchanges authorization code for tokens and creates/syncs user.
/// </summary>
public sealed class CompleteZitadelLoginCommandHandler(
    IOptions<ZitadelSettings> settings,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IHttpClientFactory httpClientFactory,
    ILogger<CompleteZitadelLoginCommandHandler> logger)
    : IRequestHandler<CompleteZitadelLoginCommand, CompleteZitadelLoginResult>
{
    private readonly ZitadelSettings _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly JwtSecurityTokenHandler _jwtHandler = new();
    private readonly ILogger<CompleteZitadelLoginCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CompleteZitadelLoginResult> Handle(
        CompleteZitadelLoginCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Completing Zitadel OAuth login");

        // 1. Exchange authorization code for tokens
        var tokenResponse = await ExchangeAuthorizationCodeAsync(
            request.AuthorizationCode,
            request.CodeVerifier,
            cancellationToken);

        if (tokenResponse.IsError)
        {
            _logger.LogError(
                "Failed to exchange authorization code: {Error} - {ErrorDescription}",
                tokenResponse.Error,
                tokenResponse.ErrorDescription);
            throw new InvalidOperationException($"Token exchange failed: {tokenResponse.Error}");
        }

        _logger.LogInformation("Successfully exchanged authorization code for tokens");

        // 2. Decode ID token to extract user claims
        var idToken = _jwtHandler.ReadJwtToken(tokenResponse.IdentityToken);
        var sub = idToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
            ?? throw new InvalidOperationException("ID token missing 'sub' claim");
        var emailClaim = idToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value
            ?? throw new InvalidOperationException("ID token missing 'email' claim");

        _logger.LogInformation(
            "Extracted user claims: sub={Sub}, email={Email}",
            sub,
            emailClaim);

        // 3. Get or create user
        var email = Email.From(emailClaim);
        var user = await GetOrCreateUserAsync(sub, email, cancellationToken);

        _logger.LogInformation(
            "User authenticated via Zitadel OAuth: UserId={UserId}, Email={Email}",
            user.Id.Value,
            email.Value);

        // 4. Return result with Zitadel's access token
        return new CompleteZitadelLoginResult
        {
            UserId = user.Id,
            Email = user.Email,
            AccessToken = tokenResponse.AccessToken!,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt
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
        var httpClient = _httpClientFactory.CreateClient();

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
    /// Gets existing user or creates new user from OAuth provider.
    /// </summary>
    private async Task<User> GetOrCreateUserAsync(
        string zitadelUserId,
        Email email,
        CancellationToken cancellationToken)
    {
        // Try to find existing user by Zitadel user ID
        var user = await _userRepository.GetByExternalUserIdAsync(
            zitadelUserId,
            "zitadel",
            cancellationToken);

        if (user != null)
        {
            _logger.LogInformation("Found existing user: UserId={UserId}", user.Id.Value);
            return user;
        }

        // Create new user from OAuth
        user = User.CreateFromOAuth(email, zitadelUserId, "zitadel");
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created new user from Zitadel OAuth: UserId={UserId}, Email={Email}",
            user.Id.Value,
            email.Value);

        return user;
    }
}
