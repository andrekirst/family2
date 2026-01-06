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
    private string? _loginHint;

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
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));

        _clientId = clientId;
        return this;
    }

    /// <summary>
    /// Sets the OAuth redirect URI (callback URL).
    /// </summary>
    public AuthorizationUrlBuilder WithRedirectUri(string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
            throw new ArgumentException("Redirect URI cannot be null or empty.", nameof(redirectUri));

        _redirectUri = redirectUri;
        return this;
    }

    /// <summary>
    /// Sets the OAuth response type (default: "code").
    /// </summary>
    public AuthorizationUrlBuilder WithResponseType(string responseType)
    {
        if (string.IsNullOrWhiteSpace(responseType))
            throw new ArgumentException("Response type cannot be null or empty.", nameof(responseType));

        _responseType = responseType;
        return this;
    }

    /// <summary>
    /// Sets the OAuth scopes (space-separated string).
    /// </summary>
    public AuthorizationUrlBuilder WithScope(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
            throw new ArgumentException("Scope cannot be null or empty.", nameof(scope));

        _scope = scope;
        return this;
    }

    /// <summary>
    /// Sets the PKCE code challenge (base64url-encoded SHA-256 hash of code verifier).
    /// </summary>
    public AuthorizationUrlBuilder WithCodeChallenge(string codeChallenge)
    {
        if (string.IsNullOrWhiteSpace(codeChallenge))
            throw new ArgumentException("Code challenge cannot be null or empty.", nameof(codeChallenge));

        _codeChallenge = codeChallenge;
        return this;
    }

    /// <summary>
    /// Sets the PKCE code challenge method (default: "S256" for SHA-256).
    /// </summary>
    public AuthorizationUrlBuilder WithCodeChallengeMethod(string codeChallengeMethod)
    {
        if (string.IsNullOrWhiteSpace(codeChallengeMethod))
            throw new ArgumentException("Code challenge method cannot be null or empty.", nameof(codeChallengeMethod));

        _codeChallengeMethod = codeChallengeMethod;
        return this;
    }

    /// <summary>
    /// Sets the state parameter for CSRF protection.
    /// </summary>
    public AuthorizationUrlBuilder WithState(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be null or empty.", nameof(state));

        _state = state;
        return this;
    }

    /// <summary>
    /// Sets the nonce parameter for replay attack protection.
    /// </summary>
    public AuthorizationUrlBuilder WithNonce(string nonce)
    {
        if (string.IsNullOrWhiteSpace(nonce))
            throw new ArgumentException("Nonce cannot be null or empty.", nameof(nonce));

        _nonce = nonce;
        return this;
    }

    /// <summary>
    /// Sets the login_hint parameter to pre-fill the login form.
    /// Used for username OR email login (Phase 5: Dual Authentication).
    /// </summary>
    public AuthorizationUrlBuilder WithLoginHint(string? loginHint)
    {
        // login_hint is optional, null/empty is valid
        _loginHint = loginHint;
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
        if (string.IsNullOrWhiteSpace(_authorizationEndpoint))
            throw new InvalidOperationException("Authorization endpoint is required.");

        if (string.IsNullOrWhiteSpace(_clientId))
            throw new InvalidOperationException("Client ID is required.");

        if (string.IsNullOrWhiteSpace(_redirectUri))
            throw new InvalidOperationException("Redirect URI is required.");

        if (string.IsNullOrWhiteSpace(_scope))
            throw new InvalidOperationException("Scope is required.");

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

        // Add optional login_hint parameter (Phase 5: Dual Authentication)
        if (!string.IsNullOrWhiteSpace(_loginHint))
        {
            queryParams.Add($"login_hint={Uri.EscapeDataString(_loginHint)}");
        }

        // Combine endpoint and query string
        return $"{_authorizationEndpoint}?{string.Join("&", queryParams)}";
    }
}
