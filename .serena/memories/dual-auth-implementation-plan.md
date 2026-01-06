# Dual Authentication & Managed Account Reliability - Implementation Plan

## Executive Summary

This plan addresses five interconnected requirements to make managed account creation more reliable and support dual authentication (username/email) with Zitadel OAuth.

**Requirements:**
1. Fix Service Account Authentication (401/403 errors)
2. Handle Zitadel Internal Errors (500/503 with retry logic)
3. Add Username Login Support (via Zitadel Actions)
4. Support Dual Authentication During Migration (manual admin control)
5. Update Login UX (unified single-input form)

**Timeline Estimate:** 3-4 weeks (single developer)
**Risk Level:** Medium (external Zitadel Actions API dependency)

---

## 1. Service Account Authentication Fixes

### Problem Analysis

Current ZitadelManagementClient issues:
- **401/403 errors**: JWT assertion may have incorrect claims or signature
- **Token caching race conditions**: Multiple threads might request new tokens simultaneously
- **Clock skew handling**: 5-minute buffer may be insufficient for some deployments
- **Permission validation**: No startup check for service account IAM roles

### Implementation

#### 1.1 Enhance JWT Assertion Generation

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Security/ZitadelManagementClient.cs`

**Changes:**
```csharp
private string CreateJwtAssertion()
{
    LogCreatingJwtAssertionForZitadelServiceAccountAuthentication();

    var privateKey = LoadPrivateKey();
    var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

    var now = DateTime.UtcNow;
    
    // ADD: Kid (Key ID) header for Zitadel to identify signing key
    var header = new JwtHeader(credentials)
    {
        { "kid", _settings.ServiceAccountKeyId } // NEW: Add from config
    };

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, _settings.ServiceAccountId),
        new Claim(JwtRegisteredClaimNames.Iss, _settings.ServiceAccountId),
        // FIX: Audience should be Zitadel's issuer, not token endpoint
        new Claim(JwtRegisteredClaimNames.Aud, _settings.Authority), // CHANGED
        new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // NEW: Prevent replay attacks
    };

    var token = new JwtSecurityToken(
        header: header, // NEW: Custom header
        claims: claims,
        notBefore: now.AddSeconds(-30), // NEW: 30-second clock skew tolerance
        expires: now.AddMinutes(5),
        signingCredentials: credentials
    );

    var assertion = new JwtSecurityTokenHandler().WriteToken(token);
    
    // ADD: Log assertion details (DO NOT log full token in production)
    LogJwtAssertionCreated(
        issuer: _settings.ServiceAccountId,
        audience: _settings.Authority,
        expiresAt: token.ValidTo
    );

    return assertion;
}
```

**New Configuration Properties:**

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Configuration/ZitadelSettings.cs`

```csharp
/// <summary>
/// Key ID from service account JSON (optional, for multi-key scenarios)
/// </summary>
public string? ServiceAccountKeyId { get; init; }

/// <summary>
/// Organization ID for service account (required for org-scoped permissions)
/// </summary>
public string OrganizationId { get; init; } = string.Empty;
```

**appsettings.json example:**
```json
{
  "Zitadel": {
    "ServiceAccountId": "123456789@family_hub",
    "ServiceAccountKeyId": "abc123def456",
    "OrganizationId": "org_abc123",
    "PrivateKeyPath": "/app/secrets/zitadel-service-account.pem"
  }
}
```

#### 1.2 Fix Token Caching Race Conditions

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Security/ZitadelManagementClient.cs`

**Changes:**
```csharp
// ADD: SemaphoreSlim for thread-safe token refresh
private static readonly SemaphoreSlim _tokenLock = new(1, 1);

private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
{
    // Check cache first (without lock)
    if (_tokenCache.TryGetValue<string>(TokenCacheKey, out var cachedToken))
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(cachedToken);
        if (jwt.ValidTo > DateTime.UtcNow.AddMinutes(TokenRefreshBufferMinutes))
        {
            LogUsingCachedZitadelAccessTokenExpiresExpiresat(jwt.ValidTo);
            return cachedToken;
        }

        LogCachedTokenExpiresSoonExpiresatRefreshing(jwt.ValidTo);
    }

    // FIX: Acquire lock to prevent multiple simultaneous token requests
    await _tokenLock.WaitAsync(cancellationToken);
    try
    {
        // Double-check pattern: Another thread might have refreshed while we waited
        if (_tokenCache.TryGetValue<string>(TokenCacheKey, out cachedToken))
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(cachedToken);
            if (jwt.ValidTo > DateTime.UtcNow.AddMinutes(TokenRefreshBufferMinutes))
            {
                LogUsingCachedZitadelAccessTokenExpiresExpiresat(jwt.ValidTo);
                return cachedToken;
            }
        }

        // Request new token with JWT bearer assertion
        LogRequestingNewZitadelAccessTokenUsingJwtAssertion();

        var assertion = CreateJwtAssertion();

        var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
            ["assertion"] = assertion,
            // FIX: Add organization scope for org-level permissions
            ["scope"] = $"openid urn:zitadel:iam:org:project:id:zitadel:aud urn:zitadel:iam:org:id:{_settings.OrganizationId}"
        });

        var response = await _httpClient.PostAsync(
            $"{_settings.Authority}/oauth/v2/token",
            requestContent,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            LogFailedToObtainZitadelAccessTokenStatusStatuscodeErrorError(response.StatusCode, errorContent);

            throw new ZitadelApiException(
                $"Failed to obtain access token from Zitadel: {errorContent}",
                response.StatusCode);
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new ZitadelApiException(
                "Zitadel returned an invalid token response.",
                HttpStatusCode.InternalServerError);
        }

        // FIX: Use absolute expiration (not sliding) to prevent indefinite caching
        var cacheExpiration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - (TokenRefreshBufferMinutes * 60));
        _tokenCache.Set(TokenCacheKey, tokenResponse.AccessToken, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheExpiration // CHANGED from sliding to absolute
        });

        LogSuccessfullyObtainedZitadelAccessTokenExpiresInExpiresinSCachedForCachesecondsS(tokenResponse.ExpiresIn, cacheExpiration.TotalSeconds);

        return tokenResponse.AccessToken;
    }
    finally
    {
        _tokenLock.Release();
    }
}
```

#### 1.3 Add Private Key Validation

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Security/ZitadelManagementClient.cs`

**Changes:**
```csharp
private RsaSecurityKey LoadPrivateKey()
{
    try
    {
        if (string.IsNullOrWhiteSpace(_settings.PrivateKeyPath))
        {
            throw new InvalidOperationException("Zitadel PrivateKeyPath is not configured.");
        }

        if (!File.Exists(_settings.PrivateKeyPath))
        {
            throw new FileNotFoundException(
                $"Zitadel private key file not found at: {_settings.PrivateKeyPath}");
        }

        var pemContent = File.ReadAllText(_settings.PrivateKeyPath);

        // FIX: Support both PKCS#1 and PKCS#8 formats
        var isPkcs8 = pemContent.Contains("BEGIN PRIVATE KEY");
        var isPkcs1 = pemContent.Contains("BEGIN RSA PRIVATE KEY");

        if (!isPkcs8 && !isPkcs1)
        {
            throw new InvalidOperationException(
                "Private key must be in PEM format (PKCS#1 or PKCS#8). " +
                "Found neither 'BEGIN PRIVATE KEY' nor 'BEGIN RSA PRIVATE KEY' header.");
        }

        // Remove PEM headers/footers
        var base64 = pemContent
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
            .Replace("-----END RSA PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim();

        var privateKeyBytes = Convert.FromBase64String(base64);
        var rsa = RSA.Create();
        
        // FIX: Try PKCS#8 first, fall back to PKCS#1
        try
        {
            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            LogPrivateKeyLoadedSuccessfully(_settings.PrivateKeyPath, "PKCS#8");
        }
        catch (CryptographicException)
        {
            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            LogPrivateKeyLoadedSuccessfully(_settings.PrivateKeyPath, "PKCS#1");
        }

        // ADD: Validate key size (Zitadel requires RSA-2048 minimum)
        var keySize = rsa.KeySize;
        if (keySize < 2048)
        {
            throw new InvalidOperationException(
                $"Private key is too weak ({keySize} bits). Zitadel requires RSA-2048 minimum.");
        }

        LogPrivateKeyValidated(keySize);

        return new RsaSecurityKey(rsa);
    }
    catch (Exception ex)
    {
        LogFailedToLoadZitadelPrivateKeyFromPath(_settings.PrivateKeyPath);
        throw new InvalidOperationException(
            $"Failed to load Zitadel private key: {ex.Message}",
            ex);
    }
}
```

#### 1.4 Add Startup Configuration Validation

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/AuthModuleServiceRegistration.cs`

**New Method:**
```csharp
public static async Task ValidateZitadelConfigurationAsync(this IServiceProvider services)
{
    var logger = services.GetRequiredService<ILogger<ZitadelManagementClient>>();
    var zitadelClient = services.GetRequiredService<IZitadelManagementClient>();

    logger.LogInformation("Validating Zitadel service account configuration...");

    try
    {
        var isValid = await zitadelClient.ValidateConnectionAsync();
        if (!isValid)
        {
            throw new InvalidOperationException(
                "Zitadel service account validation failed. Check ServiceAccountId, PrivateKeyPath, and IAM permissions.");
        }

        logger.LogInformation("Zitadel service account configuration validated successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Zitadel service account validation failed");
        throw new InvalidOperationException(
            "Failed to validate Zitadel service account. " +
            "Ensure service account has 'Org Owner' or 'User Manager' role in Zitadel.", ex);
    }
}
```

**File:** `/src/api/FamilyHub.Api/Program.cs`

**Add after `var app = builder.Build();`:**
```csharp
// Validate Zitadel configuration at startup (fail fast)
if (!app.Environment.IsDevelopment())
{
    await app.Services.ValidateZitadelConfigurationAsync();
}
```

#### 1.5 Enhanced Logging

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Security/ZitadelManagementClient.cs`

**Add logger messages:**
```csharp
[LoggerMessage(LogLevel.Debug, "Created JWT assertion: iss={issuer}, aud={audience}, exp={expiresAt}")]
partial void LogJwtAssertionCreated(string issuer, string audience, DateTime expiresAt);

[LoggerMessage(LogLevel.Information, "Private key loaded successfully from {path} (format: {format})")]
partial void LogPrivateKeyLoadedSuccessfully(string path, string format);

[LoggerMessage(LogLevel.Information, "Private key validated (key size: {keySize} bits)")]
partial void LogPrivateKeyValidated(int keySize);
```

### Testing

**New Test File:** `/src/api/tests/FamilyHub.Tests.Unit/Auth/Infrastructure/Security/ZitadelManagementClientTests.cs`

**Test Cases:**
1. `CreateJwtAssertion_ShouldIncludeKidHeader_WhenKeyIdConfigured`
2. `CreateJwtAssertion_ShouldIncludeJtiClaim_ToPreventReplayAttacks`
3. `CreateJwtAssertion_ShouldIncludeClockSkewTolerance_InNotBefore`
4. `GetAccessTokenAsync_ShouldNotRequestMultipleTokens_WhenConcurrentCalls`
5. `LoadPrivateKey_ShouldSupportPkcs1AndPkcs8_Formats`
6. `LoadPrivateKey_ShouldRejectWeakKeys_LessThan2048Bits`
7. `ValidateConnectionAsync_ShouldReturnFalse_WhenServiceAccountLacksPermissions`

---

## 2. Retry Logic Architecture

### Problem Analysis

Current implementation:
- TODO comment in CreateManagedMemberCommandHandler (line 126)
- Incomplete ManagedAccountRetryJob.cs.tmp
- No in-memory retry queue implementation
- No Polly library for resilience

### Implementation

#### 2.1 Add Polly Resilience Library

**File:** `/src/api/Directory.Packages.props`

**Add:**
```xml
<PackageVersion Include="Polly" Version="8.5.0" />
<PackageVersion Include="Polly.Extensions.Http" Version="3.0.0" />
```

#### 2.2 Create Retry Policy

**New File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Resilience/ZitadelRetryPolicy.cs`

```csharp
using Polly;
using Polly.Retry;
using System.Net;

namespace FamilyHub.Modules.Auth.Infrastructure.Resilience;

/// <summary>
/// Retry policy for Zitadel API calls with exponential backoff.
/// Retry schedule: Immediate, 2 seconds, 4 seconds (3 attempts total).
/// </summary>
public static class ZitadelRetryPolicy
{
    /// <summary>
    /// Creates a retry policy for transient Zitadel errors (500, 503, 429).
    /// </summary>
    public static AsyncRetryPolicy<HttpResponseMessage> CreateHttpRetryPolicy(ILogger logger)
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => 
                r.StatusCode == HttpStatusCode.InternalServerError ||
                r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == HttpStatusCode.TooManyRequests)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => retryAttempt switch
                {
                    1 => TimeSpan.Zero,              // Immediate
                    2 => TimeSpan.FromSeconds(2),    // 2 seconds
                    3 => TimeSpan.FromSeconds(4),    // 4 seconds
                    _ => TimeSpan.FromSeconds(8)     // Fallback
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    if (outcome.Exception != null)
                    {
                        logger.LogWarning(
                            outcome.Exception,
                            "Zitadel API call failed (attempt {RetryCount}/3). Retrying after {DelaySeconds}s. Error: {Error}",
                            retryCount,
                            timespan.TotalSeconds,
                            outcome.Exception.Message);
                    }
                    else
                    {
                        logger.LogWarning(
                            "Zitadel API returned {StatusCode} (attempt {RetryCount}/3). Retrying after {DelaySeconds}s.",
                            outcome.Result.StatusCode,
                            retryCount,
                            timespan.TotalSeconds);
                    }
                });
    }

    /// <summary>
    /// Creates a retry policy for ZitadelManagementClient operations.
    /// </summary>
    public static AsyncRetryPolicy<T> CreateGenericRetryPolicy<T>(ILogger logger)
    {
        return Policy
            .Handle<ZitadelApiException>(ex => 
                ex.StatusCode == HttpStatusCode.InternalServerError ||
                ex.StatusCode == HttpStatusCode.ServiceUnavailable ||
                ex.StatusCode == HttpStatusCode.TooManyRequests)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => retryAttempt switch
                {
                    1 => TimeSpan.Zero,
                    2 => TimeSpan.FromSeconds(2),
                    3 => TimeSpan.FromSeconds(4),
                    _ => TimeSpan.FromSeconds(8)
                },
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Zitadel operation failed (attempt {RetryCount}/3). Retrying after {DelaySeconds}s.",
                        retryCount,
                        timespan.TotalSeconds);
                });
    }
}
```

#### 2.3 Apply Retry Policy to ZitadelManagementClient

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Security/ZitadelManagementClient.cs`

**Changes:**
```csharp
public async Task<ZitadelUser> CreateHumanUserAsync(
    string username,
    string email,
    string firstName,
    string lastName,
    string password,
    CancellationToken cancellationToken = default)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(username);
    ArgumentException.ThrowIfNullOrWhiteSpace(email);
    ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
    ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
    ArgumentException.ThrowIfNullOrWhiteSpace(password);

    // NEW: Wrap in retry policy
    var retryPolicy = ZitadelRetryPolicy.CreateGenericRetryPolicy<ZitadelUser>(_logger);

    return await retryPolicy.ExecuteAsync(async () =>
    {
        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            var requestBody = new
            {
                userName = username,
                profile = new
                {
                    firstName,
                    lastName,
                    displayName = $"{firstName} {lastName}"
                },
                email = new
                {
                    email,
                    isEmailVerified = false
                },
                password
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.Authority}/management/v1/users/human")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                Content = JsonContent.Create(requestBody)
            };

            LogCreatingZitadelUserUsernameEmail(username, email);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                LogFailedToCreateZitadelUserStatusStatuscodeErrorError(response.StatusCode, errorContent);

                throw new ZitadelApiException(
                    $"Failed to create user in Zitadel: {errorContent}",
                    response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>(cancellationToken: cancellationToken);

            if (result == null || string.IsNullOrWhiteSpace(result.UserId))
            {
                throw new ZitadelApiException(
                    "Zitadel returned an invalid response (missing userId).",
                    HttpStatusCode.InternalServerError);
            }

            LogSuccessfullyCreatedZitadelUserUsernameZitadelidZitadeluserid(username, result.UserId);

            return new ZitadelUser(
                UserId: result.UserId,
                Username: username,
                Email: email);
        }
        catch (HttpRequestException ex)
        {
            LogHttpErrorWhileCreatingZitadelUserUsername(username);
            throw new ZitadelApiException(
                $"Network error while communicating with Zitadel: {ex.Message}",
                HttpStatusCode.ServiceUnavailable,
                ex);
        }
        catch (Exception ex) when (ex is not ZitadelApiException)
        {
            LogUnexpectedErrorWhileCreatingZitadelUserUsername(username);
            throw;
        }
    });
}
```

#### 2.4 Update CreateManagedMemberCommandHandler

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/CreateManagedMember/CreateManagedMemberCommandHandler.cs`

**Changes (around line 123-128):**
```csharp
// 9. Create Zitadel user with automatic retry (3 attempts)
string zitadelUserId;
try
{
    // Split person name into first and last name for Zitadel
    var nameParts = request.PersonName.Value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var firstName = nameParts.Length > 0 ? nameParts[0] : request.Username.Value;
    var lastName = nameParts.Length > 1 ? nameParts[1] : firstName; // DUPLICATE for mononyms

    // ZitadelManagementClient now includes automatic retry with exponential backoff
    // Retry schedule: Immediate, 2s, 4s (3 attempts total)
    var zitadelUser = await zitadelClient.CreateHumanUserAsync(
        username: request.Username.Value,
        email: syntheticEmail.Value,
        firstName: firstName,
        lastName: lastName,
        password: password,
        cancellationToken: cancellationToken
    );

    zitadelUserId = zitadelUser.UserId;
    LogZitadelUserCreated(zitadelUserId, request.Username.Value);
}
catch (ZitadelApiException ex) when (
    ex.StatusCode == HttpStatusCode.InternalServerError ||
    ex.StatusCode == HttpStatusCode.ServiceUnavailable)
{
    // All 3 retry attempts failed - return clear error to user
    LogZitadelCreationFailed(ex.Message);
    return Result.Failure<CreateManagedMemberResult>(
        "Failed to create Zitadel account after 3 attempts. " +
        "Zitadel may be experiencing issues. Please try again later.");
}
catch (ZitadelApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
{
    // Username already exists in Zitadel (non-retriable error)
    LogZitadelCreationFailed($"Username conflict: {ex.Message}");
    return Result.Failure<CreateManagedMemberResult>(
        $"Username '{request.Username.Value}' is already taken in Zitadel. Please choose a different username.");
}
catch (Exception ex)
{
    // Unexpected error (e.g., network timeout, serialization failure)
    LogZitadelCreationFailed(ex.Message);
    return Result.Failure<CreateManagedMemberResult>(
        $"Unexpected error creating Zitadel account: {ex.Message}");
}
```

**Note:** Removed TODO for background job queue. Synchronous retry with Polly provides:
- Immediate user feedback (no waiting for background job)
- Simpler implementation (no queue, no persistence)
- Acceptable for 3 fast retries (immediate, 2s, 4s = max 6s total)
- Background job adds complexity without significant benefit for this use case

### Testing

**New Test File:** `/src/api/tests/FamilyHub.Tests.Unit/Auth/Infrastructure/Resilience/ZitadelRetryPolicyTests.cs`

**Test Cases:**
1. `CreateHttpRetryPolicy_ShouldRetryImmediately_OnFirstFailure`
2. `CreateHttpRetryPolicy_ShouldRetryAfter2Seconds_OnSecondFailure`
3. `CreateHttpRetryPolicy_ShouldRetryAfter4Seconds_OnThirdFailure`
4. `CreateHttpRetryPolicy_ShouldNotRetry_OnNonTransientErrors` (400, 401, 403, 404)
5. `CreateGenericRetryPolicy_ShouldRetryOnZitadelApiException_With500`
6. `CreateGenericRetryPolicy_ShouldNotRetryOnZitadelApiException_With409` (Conflict)

**Integration Test:**
**File:** `/src/api/tests/FamilyHub.Tests.Integration/Auth/CreateManagedMemberWithRetryTests.cs`

```csharp
[Fact]
public async Task CreateManagedMember_ShouldRetryAndSucceed_WhenZitadelReturns503ThenSucceeds()
{
    // Arrange: Mock Zitadel to return 503 twice, then 200
    var mockHandler = new Mock<HttpMessageHandler>();
    mockHandler
        .SetupSequence<Task<HttpResponseMessage>>(/* ... */)
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"userId\": \"123\"}")
        });

    // Act
    var result = await _mediator.Send(new CreateManagedMemberCommand(/* ... */));

    // Assert
    result.IsSuccess.Should().BeTrue();
    mockHandler.Verify(/* SendAsync called 3 times */);
}
```

---

## 3. Zitadel Custom Login Action (Username OR Email)

### Problem Analysis

Zitadel's default login only accepts email addresses. To support username login:
- Must use Zitadel Actions API (JavaScript functions executed during auth flow)
- Custom action validates input format (username vs email)
- Maps username → synthetic email internally
- Returns same JWT regardless of method

### Implementation

#### 3.1 Zitadel Action Script

**New File:** `/zitadel-actions/username-email-login-action.js`

```javascript
/**
 * Zitadel Login Action: Username OR Email Authentication
 * 
 * Allows users to log in with either:
 * - Email address (e.g., john@example.com)
 * - Username (e.g., john_managed)
 * 
 * For usernames, maps to synthetic email: username@noemail.family-hub.internal
 */

function usernameOrEmailLogin(ctx, api) {
  const loginIdentifier = ctx.v1.getLoginIdentifier();
  
  // Validate input exists
  if (!loginIdentifier || loginIdentifier.trim() === '') {
    api.v1.failLogin('Please enter your email or username');
    return;
  }

  // Detect if input is email (contains @ and has valid domain)
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const isEmail = emailRegex.test(loginIdentifier);

  if (isEmail) {
    // Input is email - use as-is
    api.v1.setLoginIdentifier(loginIdentifier);
    return;
  }

  // Input is username - validate and map to synthetic email
  const usernameRegex = /^[a-zA-Z0-9_-]{3,32}$/;
  if (!usernameRegex.test(loginIdentifier)) {
    api.v1.failLogin(
      'Invalid username format. Usernames must be 3-32 characters (letters, numbers, underscore, hyphen only)'
    );
    return;
  }

  // Map username to synthetic email
  const syntheticEmail = `${loginIdentifier}@noemail.family-hub.internal`;
  api.v1.setLoginIdentifier(syntheticEmail);
  
  // Log for debugging (remove in production for privacy)
  // console.log(`Mapped username '${loginIdentifier}' to synthetic email '${syntheticEmail}'`);
}
```

#### 3.2 Deployment Script

**New File:** `/scripts/deploy-zitadel-action.sh`

```bash
#!/bin/bash
set -e

# Deploy Username/Email Login Action to Zitadel
# Requires: Zitadel CLI (zitadel-tools) or Zitadel Management API

ZITADEL_INSTANCE="${ZITADEL_INSTANCE:-http://localhost:8080}"
ORG_ID="${ZITADEL_ORG_ID:-required}"
PROJECT_ID="${ZITADEL_PROJECT_ID:-required}"
SERVICE_ACCOUNT_KEY="${SERVICE_ACCOUNT_KEY:-/path/to/service-account-key.json}"

echo "Deploying Zitadel Login Action..."
echo "Instance: $ZITADEL_INSTANCE"
echo "Organization ID: $ORG_ID"
echo "Project ID: $PROJECT_ID"

# Validate required environment variables
if [ "$ORG_ID" == "required" ] || [ "$PROJECT_ID" == "required" ]; then
  echo "Error: ZITADEL_ORG_ID and ZITADEL_PROJECT_ID are required"
  echo "Set environment variables:"
  echo "  export ZITADEL_ORG_ID='your-org-id'"
  echo "  export ZITADEL_PROJECT_ID='your-project-id'"
  exit 1
fi

# Read action script
ACTION_SCRIPT=$(cat zitadel-actions/username-email-login-action.js)

# Create action via Zitadel Management API
curl -X POST "$ZITADEL_INSTANCE/management/v1/actions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $(get-zitadel-token.sh)" \
  -d "{
    \"name\": \"username-email-login\",
    \"script\": $(echo "$ACTION_SCRIPT" | jq -Rs .),
    \"timeout\": \"10s\",
    \"allowedToFail\": false
  }"

echo "Action created successfully"

# Attach action to 'PreUserinfoCreation' flow
echo "Attaching action to login flow..."
curl -X POST "$ZITADEL_INSTANCE/management/v1/flows/0/trigger/0/actions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $(get-zitadel-token.sh)" \
  -d "{
    \"actionId\": \"<ACTION_ID_FROM_PREVIOUS_RESPONSE>\"
  }"

echo "Deployment complete!"
echo "Test username login at: $ZITADEL_INSTANCE/ui/login"
```

#### 3.3 Testing Script

**New File:** `/zitadel-actions/test-username-login.sh`

```bash
#!/bin/bash

# Test username login flow (manual verification)

ZITADEL_INSTANCE="${ZITADEL_INSTANCE:-http://localhost:8080}"

echo "Testing Username Login Flow"
echo "==========================="
echo ""
echo "1. Navigate to: $ZITADEL_INSTANCE/ui/login"
echo "2. Enter username: john_managed"
echo "3. Enter password: [managed account password]"
echo "4. Expected: Successful login"
echo ""
echo "5. Navigate to: $ZITADEL_INSTANCE/ui/login"
echo "6. Enter email: john_managed@noemail.family-hub.internal"
echo "7. Enter password: [same password]"
echo "8. Expected: Successful login (same user)"
echo ""
echo "9. Check JWT claims (both methods should produce identical JWT)"
echo "   - Use jwt.io to decode access token"
echo "   - Verify 'sub' claim is the same for both methods"
echo ""
echo "Press Enter to continue..."
read
```

### Alternative: Zitadel Actions Not Available

If Zitadel Actions API is unavailable or too complex, implement **Backend Proxy Pattern**:

**New GraphQL Mutation:**
```graphql
type Mutation {
  loginWithUsernameOrEmail(input: LoginWithUsernameOrEmailInput!): LoginWithUsernameOrEmailPayload!
}

input LoginWithUsernameOrEmailInput {
  identifier: String!  # Username OR email
  password: String!
}

type LoginWithUsernameOrEmailPayload {
  authenticationResult: AuthenticationResult
  errors: [UserError!]
}
```

**Handler Logic:**
1. Detect if input is email or username (regex)
2. If username → query database for User with matching Username
3. Extract synthetic email from User entity
4. Call Zitadel OAuth with synthetic email
5. Return JWT to frontend

**Downside:** Requires storing passwords or delegating to Zitadel's password verification API (adds complexity).

---

## 4. Dual Authentication Support

### Problem Analysis

During migration from username-only to email authentication:
- Admin adds email to managed account
- Both username and email should work for login
- Admin manually revokes username method when ready
- No automatic expiration (grace period = indefinite)

### Implementation

#### 4.1 Database Schema Changes

**Migration File:** `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Migrations/20260106000000_AddDualAuthenticationSupport.cs`

```csharp
public partial class AddDualAuthenticationSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add columns to User table
        migrationBuilder.AddColumn<string>(
            name: "real_email",
            schema: "auth",
            table: "users",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "real_email_verified",
            schema: "auth",
            table: "users",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "username_login_enabled",
            schema: "auth",
            table: "users",
            type: "boolean",
            nullable: false,
            defaultValue: true);  // Default TRUE for existing managed accounts

        // Add index for real_email lookup
        migrationBuilder.CreateIndex(
            name: "ix_users_real_email",
            schema: "auth",
            table: "users",
            column: "real_email",
            unique: true,
            filter: "real_email IS NOT NULL");

        // Add comment explaining dual auth
        migrationBuilder.Sql(@"
            COMMENT ON COLUMN auth.users.real_email IS 
                'Real email address for managed accounts during migration. ' ||
                'NULL for accounts without email. Allows dual authentication (username + email).';

            COMMENT ON COLUMN auth.users.username_login_enabled IS 
                'Whether username login is still allowed. ' ||
                'Admin can disable this after user migrates to email login.';
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_users_real_email",
            schema: "auth",
            table: "users");

        migrationBuilder.DropColumn(
            name: "real_email",
            schema: "auth",
            table: "users");

        migrationBuilder.DropColumn(
            name: "real_email_verified",
            schema: "auth",
            table: "users");

        migrationBuilder.DropColumn(
            name: "username_login_enabled",
            schema: "auth",
            table: "users");
    }
}
```

#### 4.2 Update User Entity

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Domain/User.cs`

**Add Properties:**
```csharp
/// <summary>
/// Real email address for managed accounts (null if no email provided).
/// Used during migration from username-only to email authentication.
/// </summary>
public Email? RealEmail { get; private set; }

/// <summary>
/// Whether the real email has been verified.
/// </summary>
public bool RealEmailVerified { get; private set; }

/// <summary>
/// Whether username login is still enabled.
/// Admins can disable this after user migrates to email login.
/// </summary>
public bool UsernameLoginEnabled { get; private set; } = true;
```

**Add Methods:**
```csharp
/// <summary>
/// Adds a real email to a managed account (migration path).
/// Both username and email login will be valid until admin revokes username method.
/// </summary>
public void AddRealEmail(Email realEmail)
{
    if (!IsSyntheticEmail)
    {
        throw new InvalidOperationException("Cannot add real email to non-managed account.");
    }

    if (RealEmail != null)
    {
        throw new InvalidOperationException($"Real email already set to {RealEmail.Value}");
    }

    RealEmail = realEmail;
    RealEmailVerified = false;  // Requires verification
    
    // TODO: Raise domain event for email verification flow
    // AddDomainEvent(new RealEmailAddedEvent(Id, RealEmail));
}

/// <summary>
/// Marks the real email as verified.
/// </summary>
public void VerifyRealEmail()
{
    if (RealEmail == null)
    {
        throw new InvalidOperationException("No real email to verify.");
    }

    if (RealEmailVerified)
    {
        return;
    }

    RealEmailVerified = true;
    
    // TODO: Raise domain event
    // AddDomainEvent(new RealEmailVerifiedEvent(Id, RealEmail));
}

/// <summary>
/// Disables username login (admin-controlled migration).
/// After this, user can only log in with real email.
/// </summary>
public void DisableUsernameLogin()
{
    if (!IsSyntheticEmail)
    {
        throw new InvalidOperationException("Username login is only relevant for managed accounts.");
    }

    if (RealEmail == null || !RealEmailVerified)
    {
        throw new InvalidOperationException("Cannot disable username login without verified real email.");
    }

    UsernameLoginEnabled = false;
    
    // TODO: Raise domain event
    // AddDomainEvent(new UsernameLoginDisabledEvent(Id, Username!));
}

/// <summary>
/// Re-enables username login (rollback if needed).
/// </summary>
public void EnableUsernameLogin()
{
    if (!IsSyntheticEmail)
    {
        throw new InvalidOperationException("Username login is only relevant for managed accounts.");
    }

    UsernameLoginEnabled = true;
}
```

#### 4.3 GraphQL Mutations

**New File:** `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/AddRealEmailToManagedAccount/AddRealEmailToManagedAccountCommand.cs`

```csharp
public sealed record AddRealEmailToManagedAccountCommand(
    UserId UserId,
    Email RealEmail
) : IRequest<Result<AddRealEmailToManagedAccountResult>>;

public sealed record AddRealEmailToManagedAccountResult
{
    public required UserId UserId { get; init; }
    public required Email RealEmail { get; init; }
    public required bool EmailVerified { get; init; }
}
```

**Handler:**
```csharp
public sealed class AddRealEmailToManagedAccountCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<AddRealEmailToManagedAccountCommandHandler> logger)
    : IRequestHandler<AddRealEmailToManagedAccountCommand, Result<AddRealEmailToManagedAccountResult>>
{
    public async Task<Result<AddRealEmailToManagedAccountResult>> Handle(
        AddRealEmailToManagedAccountCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Authorize: Only OWNER or ADMIN can add email to managed accounts
        var currentUserId = await currentUserService.GetUserIdAsync(cancellationToken);
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
        {
            return Result.Failure<AddRealEmailToManagedAccountResult>("Current user not found.");
        }

        if (currentUser.Role != UserRole.Owner && currentUser.Role != UserRole.Admin)
        {
            return Result.Failure<AddRealEmailToManagedAccountResult>(
                "Only OWNER or ADMIN can add email to managed accounts.");
        }

        // 2. Get target user
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<AddRealEmailToManagedAccountResult>("User not found.");
        }

        // 3. Validate user is managed account
        if (!user.IsSyntheticEmail)
        {
            return Result.Failure<AddRealEmailToManagedAccountResult>(
                "User is not a managed account. Only managed accounts can have real email added.");
        }

        // 4. Check email not already in use
        var existingUser = await userRepository.GetByEmailAsync(request.RealEmail, cancellationToken);
        if (existingUser != null)
        {
            return Result.Failure<AddRealEmailToManagedAccountResult>(
                $"Email {request.RealEmail.Value} is already in use by another user.");
        }

        // 5. Add real email
        user.AddRealEmail(request.RealEmail);

        // 6. TODO: Send verification email
        // await emailService.SendVerificationEmailAsync(request.RealEmail, cancellationToken);

        // 7. Persist
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Added real email {Email} to managed account {UserId} by {AdminUserId}",
            request.RealEmail.Value,
            request.UserId.Value,
            currentUserId.Value);

        return Result.Success(new AddRealEmailToManagedAccountResult
        {
            UserId = user.Id,
            RealEmail = request.RealEmail,
            EmailVerified = user.RealEmailVerified
        });
    }
}
```

**GraphQL Mutation:**
```graphql
type Mutation {
  addRealEmailToManagedAccount(input: AddRealEmailToManagedAccountInput!): AddRealEmailToManagedAccountPayload!
  disableUsernameLogin(input: DisableUsernameLoginInput!): DisableUsernameLoginPayload!
  enableUsernameLogin(input: EnableUsernameLoginInput!): EnableUsernameLoginPayload!
}

input AddRealEmailToManagedAccountInput {
  userId: UUID!
  realEmail: String!
}

type AddRealEmailToManagedAccountPayload {
  user: ManagedAccountUserType
  errors: [UserError!]
}

input DisableUsernameLoginInput {
  userId: UUID!
}

type DisableUsernameLoginPayload {
  success: Boolean!
  errors: [UserError!]
}
```

#### 4.4 Admin UI Component

**New File:** `/src/frontend/family-hub-web/src/app/features/family/components/manage-dual-auth-modal/manage-dual-auth-modal.component.ts`

```typescript
@Component({
  selector: 'app-manage-dual-auth-modal',
  template: `
    <div class="modal">
      <h2>Manage Authentication Methods</h2>
      
      <div class="user-info">
        <p><strong>Username:</strong> {{ user.username }}</p>
        <p><strong>Synthetic Email:</strong> {{ user.email }}</p>
        <p><strong>Real Email:</strong> {{ user.realEmail || 'Not set' }}</p>
        <p><strong>Username Login:</strong> 
          <span [class.enabled]="user.usernameLoginEnabled">
            {{ user.usernameLoginEnabled ? 'Enabled' : 'Disabled' }}
          </span>
        </p>
      </div>

      <!-- Add Real Email Section -->
      <div *ngIf="!user.realEmail" class="section">
        <h3>Add Real Email</h3>
        <p>Allow this user to log in with their email address.</p>
        <app-input
          [(value)]="newEmail"
          type="email"
          placeholder="user@example.com"
        ></app-input>
        <app-button (clicked)="addRealEmail()">Add Email</app-button>
      </div>

      <!-- Disable Username Login Section -->
      <div *ngIf="user.realEmail && user.realEmailVerified && user.usernameLoginEnabled" class="section">
        <h3>Disable Username Login</h3>
        <p>After disabling, user can only log in with email.</p>
        <p class="warning">⚠️ This action is reversible, but user must use email after this.</p>
        <app-button variant="danger" (clicked)="disableUsernameLogin()">
          Disable Username Login
        </app-button>
      </div>

      <!-- Re-enable Username Login Section -->
      <div *ngIf="!user.usernameLoginEnabled" class="section">
        <h3>Re-enable Username Login</h3>
        <p>Allow user to log in with username again.</p>
        <app-button (clicked)="enableUsernameLogin()">
          Re-enable Username Login
        </app-button>
      </div>

      <app-button variant="secondary" (clicked)="close()">Close</app-button>
    </div>
  `
})
export class ManageDualAuthModalComponent {
  @Input() user!: ManagedAccountUser;
  @Output() closed = new EventEmitter<void>();

  newEmail = '';

  constructor(private authService: AuthService) {}

  async addRealEmail(): Promise<void> {
    const result = await this.authService.addRealEmailToManagedAccount(this.user.id, this.newEmail);
    if (result.success) {
      alert('Email added successfully. Verification email sent.');
      this.closed.emit();
    } else {
      alert(`Failed: ${result.errors[0].message}`);
    }
  }

  async disableUsernameLogin(): Promise<void> {
    if (!confirm('Are you sure you want to disable username login?')) {
      return;
    }

    const result = await this.authService.disableUsernameLogin(this.user.id);
    if (result.success) {
      alert('Username login disabled. User can now only log in with email.');
      this.closed.emit();
    } else {
      alert(`Failed: ${result.errors[0].message}`);
    }
  }

  async enableUsernameLogin(): Promise<void> {
    const result = await this.authService.enableUsernameLogin(this.user.id);
    if (result.success) {
      alert('Username login re-enabled.');
      this.closed.emit();
    } else {
      alert(`Failed: ${result.errors[0].message}`);
    }
  }

  close(): void {
    this.closed.emit();
  }
}
```

### Testing

**Unit Tests:**
1. `AddRealEmail_ShouldSucceed_WhenUserIsManagedAccount`
2. `AddRealEmail_ShouldFail_WhenEmailAlreadyInUse`
3. `DisableUsernameLogin_ShouldSucceed_WhenRealEmailVerified`
4. `DisableUsernameLogin_ShouldFail_WhenRealEmailNotVerified`
5. `EnableUsernameLogin_ShouldSucceed_WhenPreviouslyDisabled`

**Integration Test:**
```csharp
[Fact]
public async Task DualAuthentication_ShouldAllowBothMethods_UntilAdminDisables()
{
    // Arrange: Create managed account with real email
    var user = await CreateManagedAccountWithEmail("john_managed", "john@example.com");

    // Act 1: Login with username
    var result1 = await LoginWithUsername("john_managed");
    result1.Should().NotBeNull();

    // Act 2: Login with email
    var result2 = await LoginWithEmail("john@example.com");
    result2.Should().NotBeNull();

    // Assert: Both produce same user ID
    result1.UserId.Should().Be(result2.UserId);

    // Act 3: Admin disables username login
    await DisableUsernameLogin(user.Id);

    // Act 4: Login with username should fail
    var result3 = await LoginWithUsername("john_managed");
    result3.Should().BeNull();

    // Act 5: Login with email should still work
    var result4 = await LoginWithEmail("john@example.com");
    result4.Should().NotBeNull();
}
```

---

## 5. Update Login UX

### Problem Analysis

Current login flow:
- Single "Sign in with Zitadel" button
- Redirects to Zitadel's login UI
- No input validation before redirect

Required changes:
- Auto-detect username vs email on frontend
- Pass hint to backend for appropriate Zitadel flow
- No user-visible tabs (seamless UX)

### Implementation

#### 5.1 Update Login Component

**File:** `/src/frontend/family-hub-web/src/app/features/auth/components/login/login.component.ts`

**Changes:**
```typescript
@Component({
  selector: 'app-login',
  imports: [ButtonComponent, InputComponent, FormsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 mb-2">
            Family Hub
          </h1>
          <p class="text-gray-600">
            Organize your family life with ease
          </p>
        </div>

        <div class="bg-white shadow-lg rounded-lg p-8">
          <h2 class="text-2xl font-semibold text-gray-900 mb-6 text-center">
            Sign In
          </h2>

          <!-- NEW: Username or Email Input -->
          <div class="mb-4">
            <app-input
              [(value)]="identifier"
              type="text"
              placeholder="Username or Email"
              [disabled]="isLoading"
              (keyup.enter)="login()"
            ></app-input>
            <p class="text-xs text-gray-500 mt-1">
              Enter your email address or username
            </p>
          </div>

          <!-- Login Button -->
          <app-button
            variant="primary"
            size="lg"
            [loading]="isLoading"
            [disabled]="!identifier || identifier.trim() === ''"
            (clicked)="login()"
            class="w-full"
          >
            Sign In
          </app-button>

          <p class="mt-4 text-sm text-gray-500 text-center">
            Secure authentication powered by Zitadel
          </p>
        </div>

        <p class="text-xs text-gray-500 text-center">
          By signing in, you agree to our Terms of Service and Privacy Policy.
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  identifier = '';  // NEW: Username or email
  isLoading = false;

  constructor() {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  async login(): Promise<void> {
    if (!this.identifier || this.identifier.trim() === '') {
      return;
    }

    try {
      this.isLoading = true;
      
      // Auto-detect if input is email or username
      const isEmail = this.detectEmailFormat(this.identifier);
      
      // Pass identifier and type to auth service
      await this.authService.loginWithIdentifier(this.identifier, isEmail ? 'email' : 'username');
      // Will redirect to Zitadel, so loading state stays true
    } catch (error) {
      console.error('Login error:', error);
      this.isLoading = false;
      alert('Login failed. Please check your username/email and try again.');
    }
  }

  private detectEmailFormat(input: string): boolean {
    // Email detection: contains @ and has valid domain structure
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(input);
  }
}
```

#### 5.2 Update AuthService

**File:** `/src/frontend/family-hub-web/src/app/core/services/auth.service.ts`

**Add Method:**
```typescript
async loginWithIdentifier(identifier: string, identifierType: 'email' | 'username'): Promise<void> {
  try {
    const query = `
      query GetZitadelAuthUrl($identifier: String!, $identifierType: String!) {
        zitadelAuthUrl(identifier: $identifier, identifierType: $identifierType) {
          authorizationUrl
          codeVerifier
          state
        }
      }
    `;

    const response = await this.graphql.query<GetZitadelAuthUrlResponse>(query, {
      identifier,
      identifierType
    });
    
    const { authorizationUrl, codeVerifier, state } = response.zitadelAuthUrl;

    // Store PKCE verifier and state in sessionStorage (temporary)
    sessionStorage.setItem('pkce_code_verifier', codeVerifier);
    sessionStorage.setItem('oauth_state', state);

    // Redirect to Zitadel OAuth UI (with login_hint if supported)
    window.location.href = authorizationUrl;
  } catch (error) {
    console.error('Login failed:', error);
    throw new Error('Failed to initiate login');
  }
}
```

#### 5.3 Update Backend Query Handler

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Application/Queries/GetZitadelAuthUrl/GetZitadelAuthUrlQuery.cs`

**Changes:**
```csharp
public sealed record GetZitadelAuthUrlQuery(
    string? Identifier,      // NEW: Username or email
    string? IdentifierType   // NEW: "email" or "username"
) : IRequest<GetZitadelAuthUrlResult>;
```

**Handler:**
```csharp
public async Task<GetZitadelAuthUrlResult> Handle(
    GetZitadelAuthUrlQuery request,
    CancellationToken cancellationToken)
{
    // Generate PKCE code verifier and challenge (same as before)
    var codeVerifier = GenerateCodeVerifier();
    var codeChallenge = GenerateCodeChallenge(codeVerifier);
    var state = GenerateState();

    // Build authorization URL with optional login_hint
    var authUrlBuilder = new UriBuilder($"{_settings.Authority}/oauth/v2/authorize")
    {
        Query = new StringBuilder()
            .Append($"client_id={Uri.EscapeDataString(_settings.ClientId)}")
            .Append($"&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}")
            .Append($"&response_type=code")
            .Append($"&scope={Uri.EscapeDataString(_settings.Scopes)}")
            .Append($"&state={Uri.EscapeDataString(state)}")
            .Append($"&code_challenge={Uri.EscapeDataString(codeChallenge)}")
            .Append($"&code_challenge_method=S256")
            .Append(BuildLoginHint(request.Identifier, request.IdentifierType))  // NEW
            .ToString()
    };

    return new GetZitadelAuthUrlResult
    {
        AuthorizationUrl = authUrlBuilder.Uri.ToString(),
        CodeVerifier = codeVerifier,
        State = state
    };
}

private string BuildLoginHint(string? identifier, string? identifierType)
{
    if (string.IsNullOrWhiteSpace(identifier))
    {
        return string.Empty;
    }

    // Map username to synthetic email for Zitadel login_hint
    if (identifierType == "username")
    {
        var syntheticEmail = $"{identifier}@noemail.family-hub.internal";
        return $"&login_hint={Uri.EscapeDataString(syntheticEmail)}";
    }

    // Email: use as-is
    if (identifierType == "email")
    {
        return $"&login_hint={Uri.EscapeDataString(identifier)}";
    }

    return string.Empty;
}
```

### Testing

**E2E Test (Playwright):**

**File:** `/src/frontend/family-hub-web/e2e/tests/username-email-login.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Username/Email Login', () => {
  test('should detect email format and redirect to Zitadel', async ({ page }) => {
    await page.goto('/login');

    // Enter email
    await page.getByPlaceholder('Username or Email').fill('john@example.com');
    await page.getByRole('button', { name: 'Sign In' }).click();

    // Should redirect to Zitadel with login_hint
    await expect(page).toHaveURL(/zitadel.*login_hint=john%40example\.com/);
  });

  test('should detect username format and map to synthetic email', async ({ page }) => {
    await page.goto('/login');

    // Enter username
    await page.getByPlaceholder('Username or Email').fill('john_managed');
    await page.getByRole('button', { name: 'Sign In' }).click();

    // Should redirect to Zitadel with synthetic email login_hint
    await expect(page).toHaveURL(/zitadel.*login_hint=john_managed%40noemail\.family-hub\.internal/);
  });

  test('should disable login button when input is empty', async ({ page }) => {
    await page.goto('/login');

    const loginButton = page.getByRole('button', { name: 'Sign In' });
    
    // Initially disabled
    await expect(loginButton).toBeDisabled();

    // Enabled after typing
    await page.getByPlaceholder('Username or Email').fill('john');
    await expect(loginButton).toBeEnabled();

    // Disabled again after clearing
    await page.getByPlaceholder('Username or Email').clear();
    await expect(loginButton).toBeDisabled();
  });
});
```

---

## 6. Testing Strategy

### 6.1 Unit Tests

**Service Account Authentication:**
- JWT assertion generation with kid, jti, clock skew
- Private key loading (PKCS#1, PKCS#8, validation)
- Token caching with SemaphoreSlim (race conditions)
- Configuration validation

**Retry Logic:**
- Polly policy retries (immediate, 2s, 4s)
- Non-retriable errors (400, 401, 403, 404, 409)
- CreateHumanUserAsync with retry wrapper

**Dual Authentication:**
- AddRealEmail (validation, uniqueness check)
- DisableUsernameLogin (requires verified email)
- EnableUsernameLogin (rollback)

**Login UX:**
- Email detection regex
- Username detection regex
- login_hint generation

### 6.2 Integration Tests

**ZitadelManagementClient:**
- CreateHumanUserAsync with mock HTTP handler
- Retry on 500/503, succeed on 3rd attempt
- No retry on 409 (Conflict)

**CreateManagedMemberCommandHandler:**
- Success path with retry
- Failure after 3 attempts (clear error message)
- Conflict error (username taken)

**Dual Authentication Flow:**
- Add real email → verify → disable username login
- Login with both methods before disabling
- Login with email only after disabling

### 6.3 E2E Tests (Playwright)

**Username Login:**
- Enter username → redirect to Zitadel with synthetic email login_hint
- Enter email → redirect to Zitadel with email login_hint

**Dual Authentication:**
- Admin adds email to managed account
- User logs in with username (success)
- User logs in with email (success)
- Admin disables username login
- User logs in with username (failure)
- User logs in with email (success)

### 6.4 Manual Testing

**Zitadel Actions:**
- Deploy action script to Zitadel
- Test username login in Zitadel UI
- Verify JWT claims are identical for username/email login

**Service Account Validation:**
- Startup validation (fail fast if misconfigured)
- Monitor logs for JWT assertion details
- Test with invalid private key (should fail gracefully)

---

## 7. Deployment & Rollback

### 7.1 Deployment Sequence

**Phase 1: Backend Changes (Week 1)**
1. Deploy service account authentication fixes
2. Deploy retry logic (Polly)
3. Deploy dual authentication database migration
4. Deploy GraphQL mutations (addRealEmail, disableUsernameLogin)
5. Run integration tests in staging
6. Monitor Seq logs for errors

**Phase 2: Zitadel Actions (Week 2)**
1. Test Zitadel action script locally
2. Deploy action to Zitadel staging instance
3. Manual testing (username/email login)
4. Deploy action to Zitadel production instance
5. Monitor Zitadel logs for action failures

**Phase 3: Frontend Changes (Week 3)**
1. Deploy login component updates
2. Deploy dual authentication admin UI
3. E2E tests with Playwright
4. User acceptance testing (UAT)

**Phase 4: Production Launch (Week 4)**
1. Blue-green deployment (backend + frontend)
2. Smoke testing (login with username, login with email)
3. Monitor error rates (Seq, Prometheus)
4. User communication (email/announcement)

### 7.2 Rollback Plan

**If Zitadel Actions fail:**
1. Disable action in Zitadel admin console
2. Fall back to email-only login
3. Investigate action logs
4. Fix action script, redeploy

**If service account auth fails:**
1. Rollback to previous Docker image
2. Check Zitadel service account permissions
3. Validate private key format and path
4. Re-deploy with fixed configuration

**If dual authentication causes issues:**
1. Rollback database migration (drop real_email columns)
2. Disable admin UI for adding emails
3. Notify users to continue using username-only login
4. Fix bugs, redeploy migration

### 7.3 Monitoring & Alerting

**Metrics to Monitor:**
- Login success rate (username vs email)
- Zitadel API error rate (401, 403, 500, 503)
- Retry policy invocations (Polly)
- Service account token refresh failures
- Dual authentication adoption rate

**Alerting Thresholds:**
- Login failure rate > 5% → Critical alert
- Zitadel API error rate > 1% → Warning
- Service account token refresh failures > 0 → Warning

**Tools:**
- Seq: Structured logging, query language
- Prometheus: Metrics collection
- Grafana: Dashboards, alerting
- Zitadel Admin Console: Action logs, user management

---

## 8. Documentation

### 8.1 ADR: Dual Authentication with Zitadel

**New File:** `/docs/architecture/ADR-005-DUAL-AUTHENTICATION-ZITADEL.md`

**Contents:**
- Context: Managed account migration from username-only to email
- Decision: Zitadel Actions API for username login, admin-controlled grace period
- Alternatives: Backend proxy, automatic expiration, Zitadel custom UI
- Consequences: Flexibility for users, admin control, no forced migration
- Security: Both methods produce same JWT, no differentiation in authorization

### 8.2 Zitadel Actions Setup Guide

**New File:** `/docs/setup/ZITADEL-ACTIONS-SETUP.md`

**Contents:**
1. Prerequisites (Zitadel instance, service account, Management API access)
2. Action script deployment (`deploy-zitadel-action.sh`)
3. Testing username login (manual verification)
4. Troubleshooting (action logs, common errors)
5. Rollback procedure

### 8.3 Admin Guide: Managing Dual Authentication

**New File:** `/docs/admin/MANAGING-DUAL-AUTHENTICATION.md`

**Contents:**
1. Overview of dual authentication
2. Adding real email to managed account (GraphQL mutation, UI walkthrough)
3. Disabling username login (when and why)
4. Re-enabling username login (rollback scenario)
5. Best practices (communicate with users, verify email first, monitor adoption)

### 8.4 User Guide: Migrating from Username to Email Login

**New File:** `/docs/user/MIGRATING-TO-EMAIL-LOGIN.md`

**Contents:**
1. Why migrate to email login?
2. How to request email addition (contact admin)
3. Email verification process
4. Using both login methods during transition
5. What happens when username login is disabled

---

## Timeline & Effort Estimation

| Phase | Tasks | Effort | Dependencies |
|-------|-------|--------|--------------|
| **Phase 1: Service Account Fixes** | JWT assertion fixes, token caching, private key validation, startup validation | 3-4 days | None |
| **Phase 2: Retry Logic** | Polly integration, ZitadelManagementClient updates, CreateManagedMemberCommandHandler changes | 2-3 days | Phase 1 |
| **Phase 3: Dual Auth Backend** | Database migration, User entity updates, GraphQL mutations, command handlers | 4-5 days | Phase 2 |
| **Phase 4: Zitadel Actions** | Action script, deployment script, manual testing, production deployment | 3-4 days | Phase 3 |
| **Phase 5: Frontend** | Login component, admin UI, AuthService updates | 3-4 days | Phase 4 |
| **Phase 6: Testing** | Unit tests, integration tests, E2E tests (Playwright) | 3-4 days | Phase 5 |
| **Phase 7: Documentation** | ADRs, setup guides, admin guides, user guides | 2-3 days | Phase 6 |
| **TOTAL** | | **20-27 days** (3-4 weeks) | |

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Zitadel Actions API unavailable/unstable | Medium | High | Fall back to backend proxy pattern (more complex, but proven) |
| Service account permission issues | Medium | High | Startup validation, clear error messages, documentation |
| Private key format incompatibility | Low | Medium | Support both PKCS#1 and PKCS#8, validation at startup |
| Token caching race conditions | Low | Medium | SemaphoreSlim double-check pattern, integration tests |
| User confusion with dual auth | Medium | Low | Clear UI messaging, admin guide, gradual rollout |
| Email verification delays | Medium | Low | Admin can manually verify, no automatic expiration |

---

## Success Criteria

1. **Service Account Authentication:**
   - Zero 401/403 errors in production logs
   - Token refresh successful 99.9% of the time
   - Startup validation catches misconfigurations

2. **Retry Logic:**
   - Managed account creation succeeds after transient Zitadel errors (500/503)
   - User sees clear error message after 3 failed attempts
   - Retry policy does not retry non-retriable errors (409, 400)

3. **Username Login:**
   - Users can log in with username OR email
   - Both methods produce identical JWT
   - Login_hint pre-fills Zitadel login form

4. **Dual Authentication:**
   - Admin can add real email to managed account
   - Admin can disable/enable username login
   - User can log in with both methods until admin revokes

5. **Login UX:**
   - Single input field (no tabs, no confusion)
   - Auto-detection works for 99% of inputs
   - Login button disabled when input is empty

---

## Next Steps After Implementation

1. **Monitor adoption:** Track percentage of managed accounts with real email
2. **User feedback:** Survey users on dual authentication experience
3. **Performance:** Monitor Zitadel API response times, retry rates
4. **Security audit:** Review Zitadel action logs for suspicious activity
5. **Documentation updates:** Incorporate learnings from production deployment

---

**END OF IMPLEMENTATION PLAN**
