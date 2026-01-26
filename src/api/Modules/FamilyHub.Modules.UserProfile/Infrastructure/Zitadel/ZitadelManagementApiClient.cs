using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace FamilyHub.Modules.UserProfile.Infrastructure.Zitadel;

/// <summary>
/// HTTP client for Zitadel Management API with Polly resilience.
/// Provides methods to get and update user profiles in Zitadel.
/// </summary>
public sealed partial class ZitadelManagementApiClient : IZitadelManagementApiClient
{
    private readonly ZitadelSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IZitadelTokenProvider _tokenProvider;
    private readonly ILogger<ZitadelManagementApiClient> _logger;
    private readonly ResiliencePipeline<HttpResponseMessage> _resiliencePipeline;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ZitadelManagementApiClient"/> class.
    /// </summary>
    public ZitadelManagementApiClient(
        IOptions<ZitadelSettings> settings,
        IHttpClientFactory httpClientFactory,
        IZitadelTokenProvider tokenProvider,
        ILogger<ZitadelManagementApiClient> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
        _logger = logger;

        _resiliencePipeline = CreateResiliencePipeline();
    }

    /// <inheritdoc />
    public async Task<ZitadelUserProfile?> GetUserProfileAsync(
        string zitadelUserId,
        CancellationToken cancellationToken = default)
    {
        LogGettingUserProfile(zitadelUserId);

        var response = await ExecuteWithResilienceAsync(
            HttpMethod.Get,
            $"/management/v1/users/{zitadelUserId}",
            null,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                LogUserNotFound(zitadelUserId);
                return null;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            LogGetUserProfileFailed(zitadelUserId, response.StatusCode.ToString(), errorContent);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var userResponse = JsonSerializer.Deserialize<ZitadelUserResponse>(content, JsonOptions);

        if (userResponse?.User?.Human?.Profile == null)
        {
            LogInvalidUserProfileResponse(zitadelUserId);
            return null;
        }

        var profile = userResponse.User.Human.Profile;
        LogUserProfileRetrieved(zitadelUserId, profile.DisplayName);

        return new ZitadelUserProfile
        {
            UserId = zitadelUserId,
            DisplayName = profile.DisplayName ?? profile.FirstName ?? "User",
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            NickName = profile.NickName,
            PreferredLanguage = profile.PreferredLanguage
        };
    }

    /// <inheritdoc />
    public async Task<bool> UpdateUserProfileAsync(
        string zitadelUserId,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        LogUpdatingUserProfile(zitadelUserId, displayName);

        var requestBody = new ZitadelUpdateProfileRequest
        {
            DisplayName = displayName
        };

        var content = JsonSerializer.Serialize(requestBody, JsonOptions);

        var response = await ExecuteWithResilienceAsync(
            HttpMethod.Put,
            $"/management/v1/users/{zitadelUserId}/profile",
            new StringContent(content, System.Text.Encoding.UTF8, "application/json"),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            LogUpdateUserProfileFailed(zitadelUserId, response.StatusCode.ToString(), errorContent);
            return false;
        }

        LogUserProfileUpdated(zitadelUserId, displayName);
        return true;
    }

    /// <summary>
    /// Executes an HTTP request with resilience pipeline.
    /// </summary>
    private async Task<HttpResponseMessage> ExecuteWithResilienceAsync(
        HttpMethod method,
        string path,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var accessToken = await _tokenProvider.GetAccessTokenAsync(cancellationToken);
        var httpClient = _httpClientFactory.CreateClient();

        var baseUrl = _settings.Authority.TrimEnd('/');
        var requestUri = $"{baseUrl}{path}";

        return await _resiliencePipeline.ExecuteAsync(async ct =>
        {
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (content != null)
            {
                request.Content = content;
            }

            var response = await httpClient.SendAsync(request, ct);

            // If we get 401, invalidate token and retry will get a new one
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _tokenProvider.InvalidateToken();
            }

            return response;
        }, cancellationToken);
    }

    /// <summary>
    /// Creates the Polly resilience pipeline for HTTP requests.
    /// </summary>
    private ResiliencePipeline<HttpResponseMessage> CreateResiliencePipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .HandleResult(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
                    .HandleResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                    .HandleResult(r => r.StatusCode == HttpStatusCode.Unauthorized), // Retry after token refresh
                OnRetry = args =>
                {
                    LogRetryAttempt(args.AttemptNumber, args.RetryDelay);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    [LoggerMessage(LogLevel.Information, "Getting user profile from Zitadel: {ZitadelUserId}")]
    partial void LogGettingUserProfile(string zitadelUserId);

    [LoggerMessage(LogLevel.Warning, "User not found in Zitadel: {ZitadelUserId}")]
    partial void LogUserNotFound(string zitadelUserId);

    [LoggerMessage(LogLevel.Error, "Failed to get user profile from Zitadel: {ZitadelUserId}, Status: {StatusCode}, Error: {ErrorContent}")]
    partial void LogGetUserProfileFailed(string zitadelUserId, string statusCode, string errorContent);

    [LoggerMessage(LogLevel.Warning, "Invalid user profile response from Zitadel: {ZitadelUserId}")]
    partial void LogInvalidUserProfileResponse(string zitadelUserId);

    [LoggerMessage(LogLevel.Debug, "Retrieved user profile from Zitadel: {ZitadelUserId}, DisplayName: {DisplayName}")]
    partial void LogUserProfileRetrieved(string zitadelUserId, string? displayName);

    [LoggerMessage(LogLevel.Information, "Updating user profile in Zitadel: {ZitadelUserId}, DisplayName: {DisplayName}")]
    partial void LogUpdatingUserProfile(string zitadelUserId, string displayName);

    [LoggerMessage(LogLevel.Error, "Failed to update user profile in Zitadel: {ZitadelUserId}, Status: {StatusCode}, Error: {ErrorContent}")]
    partial void LogUpdateUserProfileFailed(string zitadelUserId, string statusCode, string errorContent);

    [LoggerMessage(LogLevel.Information, "Successfully updated user profile in Zitadel: {ZitadelUserId}, DisplayName: {DisplayName}")]
    partial void LogUserProfileUpdated(string zitadelUserId, string displayName);

    [LoggerMessage(LogLevel.Warning, "Retrying Zitadel API request, attempt {AttemptNumber} after {Delay}")]
    partial void LogRetryAttempt(int attemptNumber, TimeSpan delay);
}
