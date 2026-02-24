using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Api.Common.Infrastructure.Configuration.Infisical;

public sealed class InfisicalConfigurationProvider : ConfigurationProvider
{
    private readonly InfisicalOptions _options;
    private readonly HttpClient _httpClient;

    public InfisicalConfigurationProvider(InfisicalOptions options)
        : this(options, new HttpClient())
    {
    }

    internal InfisicalConfigurationProvider(InfisicalOptions options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public override void Load()
    {
        LoadAsync().GetAwaiter().GetResult();
    }

    internal async Task LoadAsync()
    {
        try
        {
            var accessToken = await AuthenticateAsync();
            if (string.IsNullOrEmpty(accessToken))
                return;

            var secrets = await FetchSecretsAsync(accessToken);
            foreach (var secret in secrets)
            {
                var configKey = MapSecretKeyToConfigKey(secret.SecretKey);
                Data[configKey] = secret.SecretValue;
            }
        }
        catch (Exception)
        {
            // Graceful degradation: if Infisical is unreachable, the app
            // continues with appsettings.json defaults.
        }
    }

    private async Task<string?> AuthenticateAsync()
    {
        var loginUrl = $"{_options.Url.TrimEnd('/')}/api/v1/auth/universal-auth/login";
        var payload = new { clientId = _options.ClientId, clientSecret = _options.ClientSecret };

        var response = await _httpClient.PostAsJsonAsync(loginUrl, payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result?.AccessToken;
    }

    private async Task<List<SecretEntry>> FetchSecretsAsync(string accessToken)
    {
        var secretsUrl = $"{_options.Url.TrimEnd('/')}/api/v4/secrets" +
            $"?projectId={Uri.EscapeDataString(_options.ProjectId)}" +
            $"&environment={Uri.EscapeDataString(_options.Environment)}" +
            $"&secretPath={Uri.EscapeDataString(_options.SecretPath)}" +
            $"&viewSecretValue=true";

        using var request = new HttpRequestMessage(HttpMethod.Get, secretsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SecretsResponse>();
        return result?.Secrets ?? [];
    }

    internal static string MapSecretKeyToConfigKey(string secretKey)
    {
        // Infisical keys use mixed-case with __ as section separator:
        // GoogleIntegration__OAuth__ClientId → GoogleIntegration:OAuth:ClientId
        return secretKey.Replace("__", ":");
    }

    // --- DTOs for Infisical API responses ---

    private sealed class AuthResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = "";
    }

    private sealed class SecretsResponse
    {
        [JsonPropertyName("secrets")]
        public List<SecretEntry> Secrets { get; set; } = [];
    }

    internal sealed class SecretEntry
    {
        [JsonPropertyName("secretKey")]
        public string SecretKey { get; set; } = "";

        [JsonPropertyName("secretValue")]
        public string SecretValue { get; set; } = "";
    }
}
