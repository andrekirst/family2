namespace FamilyHub.Modules.Auth.Infrastructure.OAuth;

/// <summary>
/// Fluent builder for constructing OAuth 2.0 authorization URLs with PKCE support.
/// Encapsulates URL construction logic with proper parameter encoding and validation.
/// </summary>
public sealed class AuthorizationUrlBuilder
{
    private string? _authorizationEndpoint;
    private string? _clientId;
    private string? _redirectUri;
    private string? _responseType = "code"; // Default to authorization code flow
    private string? _scope;
    private string? _codeChallenge;
    private string? _codeChallengeMethod = "S256"; // Default to SHA-256
    private string? _state;
    private string? _nonce;

    /// <summary>
    /// Sets the OAuth authorization endpoint URL.
    /// </summary>
    public AuthorizationUrlBuilder WithAuthorizationEndpoint(string authorizationEndpoint)
    {
        if (string.IsNullOrWhiteSpace(authorizationEndpoint))
            throw new ArgumentException("Authorization endpoint cannot be null or empty.", nameof(authorizationEndpoint));

        _authorizationEndpoint = authorizationEndpoint;
        return this;
    }

    /// <summary>
    /// Sets the OAuth client ID.
    /// </summary>
    public AuthorizationUrlBuilder WithClientId(string clientId)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientId);

        _clientId = clientId;
        return this;
    }

    /// <summary>
    /// Sets the OAuth redirect URI (callback URL).
    /// </summary>
    public AuthorizationUrlBuilder WithRedirectUri(string redirectUri)
    {
        ArgumentException.ThrowIfNullOrEmpty(redirectUri);

        _redirectUri = redirectUri;
        return this;
    }

    /// <summary>
    /// Sets the OAuth response type (default: "code").
    /// </summary>
    public AuthorizationUrlBuilder WithResponseType(string responseType)
    {
        ArgumentNullException.ThrowIfNull(responseType);

        _responseType = responseType;
        return this;
    }

    /// <summary>
    /// Sets the OAuth scopes (space-separated string).
    /// </summary>
    public AuthorizationUrlBuilder WithScope(string scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        _scope = scope;
        return this;
    }

    /// <summary>
    /// Sets the PKCE code challenge (base64url-encoded SHA-256 hash of code verifier).
    /// </summary>
    public AuthorizationUrlBuilder WithCodeChallenge(string codeChallenge)
    {
        ArgumentNullException.ThrowIfNull(codeChallenge);

        _codeChallenge = codeChallenge;
        return this;
    }

    /// <summary>
    /// Sets the PKCE code challenge method (default: "S256" for SHA-256).
    /// </summary>
    public AuthorizationUrlBuilder WithCodeChallengeMethod(string codeChallengeMethod)
    {
        ArgumentNullException.ThrowIfNull(codeChallengeMethod);

        _codeChallengeMethod = codeChallengeMethod;
        return this;
    }

    /// <summary>
    /// Sets the state parameter for CSRF protection.
    /// </summary>
    public AuthorizationUrlBuilder WithState(string state)
    {
        ArgumentException.ThrowIfNullOrEmpty(state);

        _state = state;
        return this;
    }

    /// <summary>
    /// Sets the nonce parameter for replay attack protection.
    /// </summary>
    public AuthorizationUrlBuilder WithNonce(string nonce)
    {
        ArgumentNullException.ThrowIfNull(nonce);

        _nonce = nonce;
        return this;
    }

    /// <summary>
    /// Builds the complete authorization URL with all configured parameters.
    /// </summary>
    /// <returns>Fully constructed authorization URL with query parameters.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required parameters are missing.</exception>
    public string Build()
    {
        // Validate required parameters
        ArgumentNullException.ThrowIfNull(_authorizationEndpoint);
        ArgumentNullException.ThrowIfNull(_clientId);
        ArgumentNullException.ThrowIfNull(_redirectUri);

        ArgumentNullException.ThrowIfNull(_scope);

        // Build query parameters with proper URL encoding
        var queryParams = new List<string>
        {
            $"client_id={Uri.EscapeDataString(_clientId)}",
            $"redirect_uri={Uri.EscapeDataString(_redirectUri)}",
            $"response_type={_responseType}",
            $"scope={Uri.EscapeDataString(_scope!)}"
        };

        // Add optional PKCE parameters
        if (!string.IsNullOrWhiteSpace(_codeChallenge))
        {
            queryParams.Add($"code_challenge={_codeChallenge}");
            queryParams.Add($"code_challenge_method={_codeChallengeMethod}");
        }

        // Add optional state parameter
        if (!string.IsNullOrWhiteSpace(_state))
        {
            queryParams.Add($"state={_state}");
        }

        // Add optional nonce parameter
        if (!string.IsNullOrWhiteSpace(_nonce))
        {
            queryParams.Add($"nonce={_nonce}");
        }

        // Combine endpoint and query string
        return $"{_authorizationEndpoint}?{string.Join("&", queryParams)}";
    }
}
