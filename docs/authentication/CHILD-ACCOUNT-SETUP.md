# Child Account Creation Guide

**Version:** 1.0
**Last Updated:** 2026-01-04
**Status:** Implementation Ready

---

## Overview

This document describes the complete child account creation flow for Family Hub, enabling parents to create user accounts for children who do not have email addresses. Child accounts are created programmatically in Zitadel using the Management API v2, with parents setting usernames and passwords that children can use to authenticate.

### Key Features

- **No Email Required:** Children can have accounts without email addresses
- **Parent-Controlled:** Parents create and manage child credentials
- **Zitadel Integration:** Accounts created via Zitadel Management API
- **Secure by Default:** Cryptographically secure password generation
- **Synthetic Email Pattern:** Internal email format for database consistency
- **Role-Based Access:** Child role with limited permissions and parental controls

---

## Architecture Overview

```
┌─────────────────┐
│   Parent UI     │ (Family Creation Wizard or Management UI)
└────────┬────────┘
         │ Username + Full Name + Role
         ▼
┌─────────────────────────────────────────────┐
│  CreateChildMemberCommand                   │
│  (Application Layer)                        │
└────────┬────────────────────────────────────┘
         │
         ├─► 1. Validate username (3-30 chars, alphanumeric + underscore)
         │
         ├─► 2. Generate secure password (16+ chars)
         │
         ├─► 3. Create user in Zitadel via Management API
         │      ┌──────────────────────────────┐
         │      │ ZitadelManagementService     │
         │      │ - POST /management/v2/users  │
         │      │ - JWT service user auth      │
         │      └──────────────────────────────┘
         │
         ├─► 4. Create User entity in database
         │      - Username, FullName, Role: Child
         │      - Synthetic email: {username}@noemail.family-hub.internal
         │      - IsSyntheticEmail: true
         │
         ├─► 5. Add user to family with Child role
         │
         ├─► 6. Publish ChildAccountCreatedEvent
         │
         └─► 7. Return username + generated password to parent
                (Display once, copy/print options)
```

---

## Zitadel Management API Integration

### Prerequisites

1. **Zitadel Service User:** Create a service user in Zitadel with Management API access
2. **Private Key (JWT):** Generate and securely store the private key for service user authentication
3. **Required Permissions:**
   - `project.role.write` - Assign roles to newly created users
   - `user.write` - Create and manage users

### Service User Authentication

Family Hub authenticates to Zitadel using **JWT Profile for OAuth 2.0 Client Authentication**:

```csharp
public class ZitadelManagementService : IZitadelManagementService
{
    private readonly HttpClient _httpClient;
    private readonly ZitadelManagementSettings _settings;

    public async Task<string> GetAccessTokenAsync()
    {
        // 1. Load private key from secure storage
        var privateKey = await LoadPrivateKeyAsync(_settings.PrivateKeyPath);

        // 2. Create JWT assertion
        var jwt = new JwtSecurityToken(
            issuer: _settings.ServiceUserId,
            audience: _settings.Authority,
            claims: new[]
            {
                new Claim("sub", _settings.ServiceUserId),
                new Claim("iss", _settings.ServiceUserId),
                new Claim("aud", _settings.Authority)
            },
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256)
        );

        // 3. Exchange JWT for access token
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.Authority}/oauth/v2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = new JwtSecurityTokenHandler().WriteToken(jwt),
                ["scope"] = "openid profile email urn:zitadel:iam:org:project:id:{projectId}:aud"
            })
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse.AccessToken;
    }
}
```

### Creating a Child User

**API Endpoint:** `POST /management/v2/users/human`

**Request Body:**

```json
{
  "userName": "child_username",
  "profile": {
    "firstName": "Child",
    "lastName": "Name",
    "displayName": "Child Name",
    "preferredLanguage": "en"
  },
  "email": {
    "email": "child_username@noemail.family-hub.internal",
    "isEmailVerified": false
  },
  "password": {
    "password": "GeneratedSecurePassword123!",
    "changeRequired": false
  }
}
```

**Implementation:**

```csharp
public async Task<string> CreateChildUserAsync(
    Username username,
    FullName fullName,
    string generatedPassword,
    CancellationToken cancellationToken = default)
{
    var accessToken = await GetAccessTokenAsync();

    var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.ManagementApiUrl}/users/human")
    {
        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
        Content = JsonContent.Create(new
        {
            userName = username.Value,
            profile = new
            {
                firstName = fullName.FirstName,
                lastName = fullName.LastName,
                displayName = fullName.Value,
                preferredLanguage = "en"
            },
            email = new
            {
                email = $"{username.Value}@noemail.family-hub.internal",
                isEmailVerified = false
            },
            password = new
            {
                password = generatedPassword,
                changeRequired = false
            }
        })
    };

    var response = await _httpClient.SendAsync(request, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new ZitadelApiException($"Failed to create child user in Zitadel: {error}");
    }

    var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>(cancellationToken);
    return result.UserId; // Zitadel's internal user ID
}
```

---

## Username Validation Rules

Child usernames must meet the following criteria:

### Requirements

- **Length:** 3-30 characters
- **Allowed Characters:** Lowercase letters (a-z), digits (0-9), underscore (_)
- **No Special Characters:** No spaces, hyphens, or other symbols
- **Case:** Lowercase only (normalized on input)
- **Uniqueness:** Must be unique across all Family Hub users

### Implementation (Vogen Value Object)

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Username
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Username cannot be empty");

        if (value.Length < 3)
            return Validation.Invalid("Username must be at least 3 characters");

        if (value.Length > 30)
            return Validation.Invalid("Username cannot exceed 30 characters");

        if (!Regex.IsMatch(value, @"^[a-z0-9_]+$"))
            return Validation.Invalid("Username can only contain lowercase letters, digits, and underscores");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
    {
        return input?.ToLowerInvariant().Trim() ?? string.Empty;
    }

    public static Username From(string value) => From(value);
    public static bool TryFrom(string value, out Username username)
    {
        try
        {
            username = From(value);
            return true;
        }
        catch
        {
            username = default;
            return false;
        }
    }
}
```

### Examples

**Valid Usernames:**

- `child_username`
- `emma_smith_2015`
- `alex123`
- `kid_01`

**Invalid Usernames:**

- `Child_Username` (uppercase)
- `emma-smith` (hyphen not allowed)
- `al` (too short)
- `child@family` (special character)
- `my child` (space not allowed)

---

## Synthetic Email Pattern

### Purpose

- **Database Consistency:** Email field remains NOT NULL in database schema
- **Authentication Compatibility:** Zitadel requires an email field for user creation
- **Identification:** Distinguish between real and synthetic emails

### Format

```
{username}@noemail.family-hub.internal
```

**Examples:**

- `emma_kid@noemail.family-hub.internal`
- `alex_username@noemail.family-hub.internal`

### Database Representation

```csharp
public class User : AggregateRoot<UserId>
{
    public Email Email { get; private set; } // Stores synthetic email
    public Username? Username { get; private set; } // Only populated for child accounts
    public FullName? FullName { get; private set; } // Only populated for child accounts
    public bool IsSyntheticEmail { get; private set; } // Flag to identify synthetic emails
    public UserRole Role { get; private set; } // Owner, Admin, Member, Child

    public static User CreateChildFromInvitation(
        Username username,
        FullName fullName,
        UserRole role,
        string zitadelUserId)
    {
        var syntheticEmail = Email.From($"{username.Value}@noemail.family-hub.internal");

        return new User
        {
            UserId = UserId.New(),
            Email = syntheticEmail,
            IsSyntheticEmail = true,
            Username = username,
            FullName = fullName,
            Role = role,
            ZitadelUserId = zitadelUserId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

**Database Schema:**

```sql
ALTER TABLE auth.users ADD COLUMN username VARCHAR(30) UNIQUE;
ALTER TABLE auth.users ADD COLUMN full_name VARCHAR(100);
ALTER TABLE auth.users ADD COLUMN is_synthetic_email BOOLEAN DEFAULT FALSE;
ALTER TABLE auth.users ADD COLUMN role VARCHAR(50) NOT NULL DEFAULT 'member';

CREATE INDEX ix_users_username ON auth.users(username) WHERE username IS NOT NULL;
```

---

## Password Generation

### Requirements

- **Length:** Minimum 16 characters
- **Complexity:** Mix of uppercase, lowercase, digits, special characters
- **Randomness:** Cryptographically secure random number generator
- **Uniqueness:** Each password is unique
- **No Ambiguous Characters:** Avoid characters that look similar (e.g., 0/O, 1/l/I)

### Implementation

```csharp
public interface ISecurePasswordGenerator
{
    string GeneratePassword(int length = 16);
}

public class SecurePasswordGenerator : ISecurePasswordGenerator
{
    private const string LowercaseChars = "abcdefghjkmnpqrstuvwxyz"; // Exclude i, l, o
    private const string UppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // Exclude I, O
    private const string DigitChars = "23456789"; // Exclude 0, 1
    private const string SpecialChars = "!@#$%^&*-_+=";

    public string GeneratePassword(int length = 16)
    {
        if (length < 12)
            throw new ArgumentException("Password must be at least 12 characters", nameof(length));

        var password = new char[length];

        // Ensure at least one character from each category
        password[0] = GetRandomChar(LowercaseChars);
        password[1] = GetRandomChar(UppercaseChars);
        password[2] = GetRandomChar(DigitChars);
        password[3] = GetRandomChar(SpecialChars);

        // Fill remaining positions with random characters from all categories
        var allChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;
        for (int i = 4; i < length; i++)
        {
            password[i] = GetRandomChar(allChars);
        }

        // Shuffle to avoid predictable pattern
        return new string(password.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }

    private static char GetRandomChar(string chars)
    {
        var index = RandomNumberGenerator.GetInt32(chars.Length);
        return chars[index];
    }
}
```

**Example Generated Passwords:**

- `Xy9#mK2$pLqR4vT8`
- `aB3!cD5@eF7#gH9$`
- `Mn4%Pq6&Rs8*Tv2!`

---

## Security Considerations

### 1. Password Display Security

**Single Display Policy:**

- Password is shown ONLY ONCE after account creation
- Parent must copy or print the password immediately
- No password recovery mechanism (parent must reset via Zitadel if lost)

**UI Implementation:**

```typescript
// password-display-modal.component.ts
export class PasswordDisplayModalComponent {
  @Input() username: string;
  @Input() generatedPassword: string;

  copyToClipboard(): void {
    navigator.clipboard.writeText(this.generatedPassword);
    this.notificationService.success('Password copied to clipboard');
  }

  printPassword(): void {
    const printWindow = window.open('', '', 'width=400,height=300');
    if (!printWindow) return;

    const doc = printWindow.document;
    const container = doc.createElement('div');

    const title = doc.createElement('h2');
    title.textContent = 'Child Account Created';
    container.appendChild(title);

    const usernamePara = doc.createElement('p');
    const usernameLabel = doc.createElement('strong');
    usernameLabel.textContent = 'Username: ';
    usernamePara.appendChild(usernameLabel);
    usernamePara.appendChild(doc.createTextNode(this.username));
    container.appendChild(usernamePara);

    const passwordPara = doc.createElement('p');
    const passwordLabel = doc.createElement('strong');
    passwordLabel.textContent = 'Password: ';
    passwordPara.appendChild(passwordLabel);
    passwordPara.appendChild(doc.createTextNode(this.generatedPassword));
    container.appendChild(passwordPara);

    const warning = doc.createElement('p');
    const em = doc.createElement('em');
    em.textContent = 'Keep this information secure.';
    warning.appendChild(em);
    container.appendChild(warning);

    doc.body.appendChild(container);
    printWindow.print();
  }
}
```

### 2. Parent Responsibilities

**Parents Must:**

- Save the generated password securely (password manager, secure note)
- Teach children to keep credentials confidential
- Monitor child's account activity (via parental controls - Phase 3+)
- Update password periodically via Zitadel

**Warning Message (Displayed in UI):**

```
⚠️ IMPORTANT: Save this password securely. It won't be shown again.

As a parent, you are responsible for:
• Keeping this password in a secure location
• Teaching your child to protect their credentials
• Updating the password if compromised
```

### 3. Access Control

**Child Role Permissions (Enforced in Authorization Layer):**

| Feature | Child Permission |
|---------|------------------|
| View family calendar | ✅ Read-only |
| Create calendar events | ❌ Denied |
| View own tasks | ✅ Allowed |
| Complete own tasks | ✅ Allowed |
| Assign tasks to others | ❌ Denied |
| View shopping lists | ✅ Read-only |
| Add to shopping lists | ⚠️ With approval (Phase 3+) |
| View health records | ⚠️ Own records only |
| Manage family settings | ❌ Denied |
| Invite family members | ❌ Denied |

### 4. Private Key Security

**Service User Private Key Storage:**

- **Development:** Store in `appsettings.Development.json` (NOT committed to git)
- **Production:** Store in Azure Key Vault, AWS Secrets Manager, or Kubernetes Secrets
- **File Permissions:** Read-only for application user (chmod 400)
- **Rotation:** Rotate service user keys every 90 days

**Example Configuration:**

```json
{
  "ZitadelManagement": {
    "Authority": "https://family-hub.zitadel.cloud",
    "ServiceUserId": "service-user-id",
    "ServiceUserKeyId": "key-id",
    "PrivateKeyPath": "/secrets/zitadel-service-key.json",
    "OrganizationId": "org-id",
    "ProjectId": "project-id",
    "ManagementApiUrl": "https://family-hub.zitadel.cloud/management/v2"
  }
}
```

---

## Error Handling

### Common Errors and Resolutions

| Error | Cause | Resolution |
|-------|-------|------------|
| `USERNAME_ALREADY_EXISTS` | Username taken in Zitadel | Prompt user to choose different username |
| `INVALID_USERNAME_FORMAT` | Username validation failed | Show validation error, enforce format rules |
| `ZITADEL_API_UNAUTHORIZED` | Service user token expired | Refresh access token and retry |
| `ZITADEL_API_RATE_LIMIT` | Too many API requests | Implement exponential backoff, retry after delay |
| `NETWORK_ERROR` | Connection to Zitadel failed | Retry with exponential backoff, alert user |

### Implementation

```csharp
public class CreateChildMemberCommandHandler : ICommandHandler<CreateChildMemberCommand, CreateChildMemberResult>
{
    public async Task<CreateChildMemberResult> Handle(
        CreateChildMemberCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate username availability
            var existingUser = await _userRepository.GetByUsernameAsync(command.Username, cancellationToken);
            if (existingUser != null)
                throw new UsernameAlreadyExistsException(command.Username);

            // 2. Generate secure password
            var generatedPassword = _passwordGenerator.GeneratePassword(16);

            // 3. Create user in Zitadel (with retry logic)
            string zitadelUserId;
            try
            {
                zitadelUserId = await _zitadelManagementService.CreateChildUserAsync(
                    command.Username,
                    command.FullName,
                    generatedPassword,
                    cancellationToken
                );
            }
            catch (ZitadelApiException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Rate limit - wait and retry
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                zitadelUserId = await _zitadelManagementService.CreateChildUserAsync(
                    command.Username,
                    command.FullName,
                    generatedPassword,
                    cancellationToken
                );
            }

            // 4. Create User entity
            var user = User.CreateChildFromInvitation(
                command.Username,
                command.FullName,
                UserRole.Child,
                zitadelUserId
            );

            await _userRepository.AddAsync(user, cancellationToken);

            // 5. Add to family
            var family = await _familyRepository.GetByIdAsync(command.FamilyId, cancellationToken);
            family.AddMember(user.UserId, UserRole.Child);

            // 6. Publish domain event
            user.PublishDomainEvent(new ChildAccountCreatedEvent(
                InvitationId: command.InvitationId,
                FamilyId: command.FamilyId,
                ChildUserId: user.UserId,
                Username: command.Username,
                FullName: command.FullName,
                Role: UserRole.Child,
                CreatedByUserId: command.CreatedByUserId,
                ZitadelUserId: zitadelUserId
            ));

            await _unitOfWork.CommitAsync(cancellationToken);

            return new CreateChildMemberResult(
                user.UserId,
                command.Username,
                generatedPassword,
                zitadelUserId
            );
        }
        catch (UsernameAlreadyExistsException ex)
        {
            throw new DomainException("Username is already taken. Please choose a different username.", ex);
        }
        catch (ZitadelApiException ex)
        {
            throw new InfrastructureException("Failed to create child account in Zitadel. Please try again later.", ex);
        }
    }
}
```

---

## Testing Strategy

### Unit Tests

**Test Cases:**

1. **Username Validation:**
   - Valid usernames (lowercase, digits, underscores)
   - Invalid usernames (uppercase, special chars, too short/long)
   - Normalization (uppercase input → lowercase output)

2. **Password Generation:**
   - Length requirements (minimum 16 characters)
   - Complexity requirements (all character categories present)
   - Uniqueness (multiple generations produce different passwords)
   - No ambiguous characters

3. **Synthetic Email Format:**
   - Correct format: `{username}@noemail.family-hub.internal`
   - IsSyntheticEmail flag set correctly
   - Email value object validation

4. **Command Handler Logic:**
   - Username availability check
   - User creation in database
   - Family member addition
   - Domain event publishing

### Integration Tests

**Test Scenarios:**

1. **End-to-End Child Account Creation:**
   - Zitadel API call (sandbox environment)
   - Database record creation
   - Event publishing to event bus
   - GraphQL mutation response

2. **Error Scenarios:**
   - Duplicate username handling
   - Zitadel API failures (unauthorized, rate limit, network error)
   - Database transaction rollback on error

3. **GraphQL API Tests:**
   - `createChildMember` mutation with valid input
   - Input validation errors
   - Authorization (only Owner/Admin can create child accounts)

**Example Integration Test:**

```csharp
[Fact]
public async Task CreateChildMember_ValidInput_CreatesUserInZitadelAndDatabase()
{
    // Arrange
    var command = new CreateChildMemberCommand(
        FamilyId: _testFamilyId,
        Username: Username.From("test_child_123"),
        FullName: FullName.From("Test Child"),
        Role: UserRole.Child,
        CreatedByUserId: _testParentUserId,
        InvitationId: InvitationId.New()
    );

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Username.Should().Be(command.Username);
    result.GeneratedPassword.Should().HaveLength(16);

    // Verify user created in database
    var user = await _userRepository.GetByIdAsync(result.ChildUserId);
    user.Should().NotBeNull();
    user.Username.Should().Be(command.Username);
    user.IsSyntheticEmail.Should().BeTrue();
    user.Email.Value.Should().Be("test_child_123@noemail.family-hub.internal");

    // Verify domain event published
    var events = _eventBus.PublishedEvents.OfType<ChildAccountCreatedEvent>();
    events.Should().ContainSingle();
}
```

---

## Monitoring and Observability

### Metrics to Track

1. **Child Account Creation Rate:**
   - Total child accounts created per day/week
   - Success vs failure rate
   - Average creation time

2. **Zitadel API Performance:**
   - API request latency (p50, p95, p99)
   - Rate limit violations
   - Error rates by error type

3. **Password Generation:**
   - Generation time (should be <50ms)
   - Distribution analysis (entropy check)

### Logging

**Log Levels:**

```csharp
// INFO: Successful account creation
_logger.LogInformation(
    "Child account created successfully. UserId: {UserId}, Username: {Username}, ZitadelUserId: {ZitadelUserId}",
    user.UserId, command.Username, zitadelUserId
);

// WARNING: Rate limit approaching
_logger.LogWarning(
    "Zitadel API rate limit warning. Requests remaining: {RemainingRequests}",
    remainingRequests
);

// ERROR: Account creation failed
_logger.LogError(ex,
    "Failed to create child account. Username: {Username}, Error: {ErrorMessage}",
    command.Username, ex.Message
);
```

**Sensitive Data Handling:**

- **NEVER log passwords** (generated or user-provided)
- **NEVER log private keys** or access tokens (log only token expiration)
- **Mask email addresses** in logs: `user***@example.com`

---

## Future Enhancements

### Phase 2+

1. **Email Verification for Children (Optional):**
   - Allow adding email later for notifications
   - Verify email without changing authentication method

2. **Password Reset Flow:**
   - Parent-initiated password reset via Zitadel
   - Email-based reset for children with verified emails

3. **Multi-Factor Authentication (MFA):**
   - Support for TOTP (Google Authenticator, Authy)
   - Parental oversight of MFA setup

4. **Parental Controls:**
   - Screen time limits
   - Content filtering
   - Activity monitoring

### Phase 5+

1. **Self-Service Password Change:**
   - Children can change their own passwords (with parent notification)
   - Password strength meter and requirements enforcement

2. **Account Graduation:**
   - Convert child account to full member when they turn 18
   - Add email and enable full permissions

---

## References

### Official Documentation

- [Zitadel Management API v2](https://zitadel.com/docs/apis/resources/mgmt/management-service-v2)
- [Create Human User - Zitadel Docs](https://zitadel.com/docs/apis/resources/user_service_v2/user-service-add-human-user)
- [Authenticate Service Users - Zitadel](https://zitadel.com/docs/guides/integrate/service-users/authenticate-service-users)
- [OAuth 2.0 JWT Profile - RFC 7523](https://tools.ietf.org/html/rfc7523)

### Related Documentation

- [ADR-002: OAuth with Zitadel](../architecture/ADR-002-OAUTH-WITH-ZITADEL.md)
- [Domain Model - Auth Service](../architecture/domain-model-microservices-map.md#auth-service)
- [Implementation Roadmap - Phase 1](../product-strategy/implementation-roadmap.md#phase-1-core-mvp)
- [Feature Backlog - Family Member Invites](../product-strategy/FEATURE_BACKLOG.md)

---

**Document Ownership:**
Architecture & Development Team

**Review Cycle:**
Quarterly or upon architecture changes
