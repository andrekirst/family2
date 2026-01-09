# Security Patterns Guide

**Purpose:** Guide to security architecture, threat model, OWASP testing, Row-Level Security, and vulnerability management in Family Hub.

**Tech Stack:** PostgreSQL RLS, Zitadel OAuth 2.0, HTTPS/TLS 1.3, STRIDE threat modeling

---

## Quick Reference

### Core Security Documents

1. **[threat-model.md](threat-model.md)** - STRIDE analysis, 12 critical threats
2. **[security-testing-plan.md](security-testing-plan.md)** - OWASP Top 10 testing
3. **[vulnerability-management.md](vulnerability-management.md)** - CVE tracking, patching
4. **[security-monitoring-incident-response.md](security-monitoring-incident-response.md)** - Monitoring and incident response

---

## Critical Patterns (3)

### 1. Row-Level Security (RLS) for Multi-Tenancy

**PostgreSQL RLS** enforces data isolation at database level.

**Implementation:**

```sql
-- Enable RLS on table
ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

-- Policy: User can only see their own data
CREATE POLICY user_isolation_policy ON auth.users
    USING (id = current_setting('app.current_user_id', true)::uuid);

-- Policy: User can see their family members
CREATE POLICY family_isolation_policy ON auth.users
    USING (family_id = current_setting('app.current_family_id', true)::uuid);
```

**Setting Session Variables:**

```csharp
// In DbContext or HTTP interceptor
await connection.ExecuteAsync(
    "SELECT set_config('app.current_user_id', @userId, false)",
    new { userId = currentUserId.ToString() }
);

await connection.ExecuteAsync(
    "SELECT set_config('app.current_family_id', @familyId, false)",
    new { familyId = currentFamilyId.ToString() }
);
```

**Benefits:**

- **Defense in depth** - Database enforces isolation even if app logic fails
- **GDPR compliance** - Strong data isolation per family
- **Performance** - Database-level filtering
- **Auditability** - Cannot bypass RLS policies

**Testing RLS:**

```sql
-- Test policy works
SET app.current_user_id = '00000000-0000-0000-0000-000000000001';
SELECT * FROM auth.users;  -- Should only return current user's data

-- Test policy blocks unauthorized access
SET app.current_family_id = '00000000-0000-0000-0000-000000000999';
SELECT * FROM auth.users WHERE family_id = current_setting('app.current_family_id')::uuid;
-- Should return empty (no access to other families)
```

---

### 2. OAuth 2.0 Security (Zitadel + PKCE)

**PKCE (Proof Key for Code Exchange)** prevents authorization code interception.

**Flow:**

```
1. Generate code_verifier (random 32-byte string)
2. Generate code_challenge = SHA256(code_verifier)
3. Redirect to Zitadel with code_challenge
4. User authenticates
5. Redirect back with authorization code
6. Exchange code + code_verifier for tokens
```

**Implementation:**

```typescript
// Generate PKCE parameters
const codeVerifier = generateCodeVerifier();
const codeChallenge = await generateCodeChallenge(codeVerifier);
const state = generateState();

// Store for callback
sessionStorage.setItem('code_verifier', codeVerifier);
sessionStorage.setItem('state', state);

// Redirect to Zitadel
const authUrl = `${zitadelIssuer}/oauth/v2/authorize?` +
  `client_id=${clientId}&` +
  `redirect_uri=${redirectUri}&` +
  `response_type=code&` +
  `scope=openid profile email&` +
  `code_challenge=${codeChallenge}&` +
  `code_challenge_method=S256&` +
  `state=${state}`;

window.location.href = authUrl;
```

**Security Checks:**

- ✅ **State parameter validation** (prevents CSRF)
- ✅ **PKCE code verifier** (prevents code interception)
- ✅ **HTTPS only** (prevents MitM attacks)
- ✅ **Token refresh rotation** (limits token lifetime)
- ✅ **Secure token storage** (HttpOnly cookies preferred)

**See:** [../authentication/OAUTH_INTEGRATION_GUIDE.md](../authentication/OAUTH_INTEGRATION_GUIDE.md)

---

### 3. OWASP Top 10 Testing

**Critical Security Tests:**

**A01: Broken Access Control**

- Test RLS policies prevent cross-family data access
- Test GraphQL mutations require authentication
- Test users cannot modify other users' data

**A02: Cryptographic Failures**

- Test HTTPS enforced (no HTTP)
- Test TLS 1.3 minimum
- Test passwords never stored (OAuth only)
- Test sensitive data encrypted at rest

**A03: Injection**

- Test SQL injection via GraphQL parameters
- Test XSS via user input fields
- Test command injection (unlikely with C#)

**A04: Insecure Design**

- Review threat model for design flaws
- Test event chains for race conditions
- Test multi-tenant isolation

**A05: Security Misconfiguration**

- Test default credentials changed
- Test debug mode disabled in production
- Test security headers present (CSP, HSTS, X-Frame-Options)

**A06: Vulnerable Components**

- Run `dotnet list package --vulnerable`
- Run `npm audit`
- Track CVEs for PostgreSQL, RabbitMQ, Zitadel

**A07: Identification & Authentication**

- Test OAuth flow (PKCE, state validation)
- Test token expiration
- Test logout clears tokens

**A08: Software & Data Integrity**

- Test CI/CD pipeline integrity
- Test code signing (future)
- Test database backups

**A09: Security Logging & Monitoring**

- Test failed login attempts logged
- Test suspicious activity detected
- Test alerts sent for critical events

**A10: Server-Side Request Forgery (SSRF)**

- Test URL validation for user-provided URLs
- Test webhook validation

**See:** [security-testing-plan.md](security-testing-plan.md) for comprehensive test plan.

---

## Threat Model Summary (STRIDE)

**Critical Threats:**

| Threat | Category | Severity | Mitigation |
|--------|----------|----------|------------|
| Cross-family data access | Spoofing | Critical | PostgreSQL RLS policies |
| OAuth token theft | Tampering | Critical | PKCE + HTTPS + secure storage |
| SQL injection via GraphQL | Injection | High | Parameterized queries, input validation |
| Unauthorized API access | Elevation | High | JWT validation, rate limiting |
| Health data breach | Disclosure | Critical | End-to-end encryption |
| Event replay attacks | Tampering | Medium | Event deduplication, timestamps |

**Full Analysis:** [threat-model.md](threat-model.md) - 12 critical, 18 high, 23 medium threats

---

## Security Best Practices

### Input Validation

**Always validate at multiple layers:**

1. **Client-side** (UX convenience)
2. **GraphQL Input DTOs** (framework validation)
3. **MediatR Commands** (FluentValidation)
4. **Domain entities** (Vogen value objects)
5. **Database** (constraints, RLS)

**Example:**

```csharp
// Vogen validation (domain)
[ValueObject<string>]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format");
        return Validation.Ok;
    }
}

// FluentValidation (application)
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
    }
}

// Database constraint (persistence)
builder.Property(u => u.Email)
    .HasMaxLength(320)
    .IsRequired();
```

---

### Secrets Management

**Never commit secrets:**

- ✅ Use environment variables
- ✅ Use .env files (gitignored)
- ✅ Use Azure Key Vault (production)
- ✅ Rotate secrets regularly
- ❌ No secrets in code
- ❌ No secrets in git history

**Example (.env file):**

```bash
# Development only (never commit)
ConnectionStrings__DefaultConnection=Host=localhost;...
Zitadel__ClientSecret=supersecret123
RabbitMQ__Password=Dev123!
```

---

### Security Headers

**Required HTTP headers:**

```csharp
// Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");

    await next();
});
```

---

## Compliance

### GDPR (General Data Protection Regulation)

- ✅ Data minimization (collect only necessary data)
- ✅ Right to access (user can export data)
- ✅ Right to deletion (user can delete account)
- ✅ Data portability (GraphQL export)
- ✅ Privacy by design (RLS, encryption)

### COPPA (Children's Online Privacy Protection Act)

- ✅ Parental consent required for children < 13
- ✅ No behavioral advertising for children
- ✅ Data retention limited
- ✅ Strong data security (RLS, encryption)

**See:** [../legal/LEGAL-COMPLIANCE-SUMMARY.md](../legal/LEGAL-COMPLIANCE-SUMMARY.md)

---

## Security Monitoring

**Audit Logging:**

```csharp
_logger.LogWarning(
    "Failed login attempt for user {Email} from {IPAddress}",
    email, ipAddress);

_logger.LogInformation(
    "User {UserId} accessed family {FamilyId} data",
    userId, familyId);
```

**Alerts:**

- Failed login attempts (> 5 in 10 minutes)
- Cross-family data access attempts
- Suspicious SQL patterns
- Large data exports

**See:** [security-monitoring-incident-response.md](security-monitoring-incident-response.md)

---

## Related Documentation

- **Backend Guide:** [../../src/api/CLAUDE.md](../../src/api/CLAUDE.md) - Security patterns in code
- **Database Guide:** [../../database/CLAUDE.md](../../database/CLAUDE.md) - RLS implementation
- **Authentication:** [../authentication/OAUTH_INTEGRATION_GUIDE.md](../authentication/OAUTH_INTEGRATION_GUIDE.md) - OAuth security
- **Architecture:** [../architecture/CLAUDE.md](../architecture/CLAUDE.md) - Multi-tenancy strategy

---

**Last Updated:** 2026-01-09
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- threat-model.md (STRIDE analysis, threat identification)
- security-testing-plan.md (OWASP Top 10 testing)
- ../architecture/multi-tenancy-strategy.md (RLS implementation)
- ../authentication/OAUTH_INTEGRATION_GUIDE.md (OAuth security)

**Sync Checklist:**

- [ ] RLS examples match multi-tenancy-strategy.md
- [ ] OAuth patterns match OAUTH_INTEGRATION_GUIDE.md
- [ ] Threat categories match threat-model.md
- [ ] OWASP coverage matches security-testing-plan.md
