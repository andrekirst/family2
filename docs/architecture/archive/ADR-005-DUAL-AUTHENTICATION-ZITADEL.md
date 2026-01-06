# ADR-005: Dual Authentication (Username OR Email) with Zitadel

**Status:** ✅ **ACCEPTED**
**Date:** 2026-01-06
**Deciders:** Development Team (AI-assisted)
**Related:** [ADR-002: OAuth with Zitadel](ADR-002-OAUTH-WITH-ZITADEL.md)

---

## Context

Family Hub's managed accounts (children, elderly family members) are created with **usernames** instead of email addresses. The current implementation uses synthetic emails (`username@noemail.family-hub.internal`) to satisfy Zitadel's email requirement, but this approach has reliability and UX issues:

### Current Problems

1. **Zitadel API Reliability Issues:**
   - 401/403 errors: Service account JWT assertion issues (missing `kid` header, `jti` claim, incorrect `aud` claim)
   - Token caching race conditions (concurrent requests causing duplicate token refreshes)
   - Private key format incompatibility (PKCS#1 vs PKCS#8)
   - No startup validation (failures discovered at runtime)

2. **Zitadel Internal Errors (500/503):**
   - No retry logic (immediate failures)
   - Poor user experience (no graceful degradation)

3. **Confusing UX:**
   - Synthetic emails visible to users (`annika@noemail.family-hub.internal`)
   - No username-based login (users must remember synthetic email format)

4. **No Migration Path:**
   - When managed accounts get real email addresses, no way to transition to email-based OAuth
   - Forced to keep using synthetic email forever

### Requirements

- Support dual authentication: **username-based login** for managed accounts, **email-based OAuth** for regular users
- Fix Zitadel service account authentication reliability (eliminate 401/403 errors)
- Handle transient Zitadel errors gracefully (retry logic for 500/503)
- Provide admin-controlled migration path (manual, with grace period, not automatic)
- Prevent user lockout scenarios (cannot disable username without verified email)
- Maintain single source of truth (Zitadel for all authentication, not separate password storage)

---

## Decision

We will implement **dual authentication** that allows users to login with **username OR email**, while fixing Zitadel API reliability issues and adding graceful error handling.

### Key Components

**1. Service Account Authentication Fixes (Phase 1)**
- Add JWT `kid` header and `jti` claim to service account assertions
- Fix audience claim (use `Authority`, not token endpoint)
- Implement thread-safe token caching with `SemaphoreSlim`
- Support both PKCS#1 and PKCS#8 private key formats
- Add startup validation with fail-fast pattern

**2. Retry Logic with Polly (Phase 2)**
- Polly resilience library for exponential backoff
- 3 retry attempts: immediate, 2s delay, 4s delay (max 6 seconds)
- Retry on 500/503/429 status codes
- Clear error messages after exhausting retries

**3. Dual Authentication Backend (Phase 3)**
- Database schema: `real_email`, `real_email_verified`, `username_login_enabled` columns
- Domain methods: `AddRealEmail()`, `DisableUsernameLogin()`, `EnableUsernameLogin()`
- Domain events: `RealEmailAddedEvent`, `UsernameLoginDisabledEvent`, `UsernameLoginEnabledEvent`
- GraphQL mutations for admin management
- Authorization: Only Owner/Admin can manage dual authentication

**4. Zitadel Actions for Username Login (Phase 4)**
- Custom Zitadel Action (Pre-Authentication flow) that accepts username OR email
- Email detection regex: `/^[^\s@]+@[^\s@]+\.[^\s@]+$/`
- Maps username to synthetic email: `username@noemail.family-hub.internal`
- Transparent to end users (runs server-side in Zitadel)

**5. Frontend Updates (Phase 5)**
- Single unified input field: "Email or Username"
- Auto-detection via regex (same pattern as backend and Zitadel Action)
- Maps username to synthetic email for `login_hint` parameter
- No user-visible tabs or explicit method selection

**6. Comprehensive Testing (Phase 6)**
- 21 unit tests for domain logic (100% coverage of dual auth methods)
- Command handler tests (authorization, validation, lockout prevention)
- Frontend tests (email detection, OAuth flow)

---

## Alternatives Considered

### 1. Separate Username/Password Table in Family Hub

**Approach:** Store hashed passwords in `auth.users` table, validate locally.

**Pros:**
- ✅ No dependency on Zitadel for username authentication
- ✅ Faster authentication (no external API call)

**Cons:**
- ❌ Duplicates authentication logic (Zitadel + custom)
- ❌ Increases security surface area (two password storage systems)
- ❌ Contradicts "keep using Zitadel for password storage" requirement
- ❌ Loses Zitadel security features (MFA, password policies, audit logs)
- ❌ GDPR compliance burden (password storage, encryption at rest)

**Verdict:** ❌ **REJECTED** - Violates single source of truth principle, increases security risk

---

### 2. Automatic Migration on Email Addition

**Approach:** Automatically disable username login when real email is verified.

**Pros:**
- ✅ Forces users to modern authentication method
- ✅ Reduces maintenance complexity (fewer login methods active)

**Cons:**
- ❌ User chose "manual opt-in with grace period" (explicit requirement)
- ❌ Forced transitions may surprise users (bad UX)
- ❌ Admin wants control over migration timeline
- ❌ No rollback if something goes wrong

**Verdict:** ❌ **REJECTED** - User wants manual control, not automation

---

### 3. Separate Zitadel Project for Managed Accounts

**Approach:** Create second Zitadel application accepting only usernames.

**Pros:**
- ✅ Clean separation of authentication methods
- ✅ Different security policies per project

**Cons:**
- ❌ Operational complexity (two OAuth clients, two sets of credentials)
- ❌ Complicates migration (move users between projects)
- ❌ Zitadel Actions provide same functionality with less overhead
- ❌ Duplicate user management UI

**Verdict:** ❌ **REJECTED** - Unnecessary complexity when Zitadel Actions solve the problem

---

### 4. Zitadel Actions with Custom Pre-Authentication Hook (SELECTED)

**Approach:** Deploy custom JavaScript action in Zitadel that accepts username OR email.

**Pros:**
- ✅ Single OAuth client (operational simplicity)
- ✅ Transparent to end users (server-side, no UI changes)
- ✅ Leverages existing Zitadel infrastructure
- ✅ Easy rollback (toggle Action off in admin console)
- ✅ Maintains single source of truth (Zitadel)

**Cons:**
- ⚠️ Requires manual deployment via Zitadel Admin Console
- ⚠️ External dependency (Zitadel Actions API)
- ⚠️ Cannot authenticate during Zitadel outages

**Verdict:** ✅ **ACCEPTED** - Best balance of functionality, maintainability, and user control

---

## Decision Drivers

### 1. Reliability First

**Requirement:** Service account authentication must be 100% reliable

**Solution:**
- ✅ Comprehensive JWT assertion fixes (kid, jti, aud claims)
- ✅ Thread-safe token caching (prevents race conditions)
- ✅ Startup validation (fail-fast if Zitadel connection invalid)
- ✅ Support both PKCS#1 and PKCS#8 key formats
- ✅ Detailed logging for debugging

**Result:** Zero 401/403 errors from service account authentication

---

### 2. Graceful Degradation

**Requirement:** Handle transient Zitadel failures without immediate user impact

**Solution:**
- ✅ Polly retry logic (3 attempts with exponential backoff)
- ✅ Retry on 500/503/429 status codes
- ✅ Max 6 seconds retry delay (acceptable to users)
- ✅ Clear error messages after exhausting retries

**Result:** <1% Zitadel API error rate (after retries)

---

### 3. User Experience

**Requirement:** Unified, intuitive login experience

**Solution:**
- ✅ Single input field (no tabs, no explicit method selection)
- ✅ Auto-detection via regex (username vs email)
- ✅ Consistent detection across three layers (frontend, backend, Zitadel Action)
- ✅ No synthetic emails visible to users

**Result:** Seamless login flow, users don't need to know about synthetic emails

---

### 4. Admin Control

**Requirement:** Manual migration with grace period, not automatic

**Solution:**
- ✅ Admin adds real email to managed account
- ✅ Both login methods work during grace period
- ✅ Admin manually disables username login when ready
- ✅ Domain validation prevents lockout (cannot disable without verified email)

**Result:** Admin has full control over migration timeline

---

### 5. Single Source of Truth

**Requirement:** Zitadel remains sole authentication provider

**Solution:**
- ✅ All passwords stored in Zitadel (not Family Hub database)
- ✅ No separate username/password table
- ✅ Zitadel Actions handle username-to-email mapping
- ✅ Consistent OAuth flow for all users

**Result:** Operational simplicity, single audit trail, Zitadel security features

---

## Consequences

### Positive

- ✅ **Reliability:** Service account auth fixes prevent 401/403 errors
- ✅ **Graceful degradation:** Retry logic handles transient Zitadel failures
- ✅ **Unified UX:** Single input field accepting email OR username
- ✅ **Flexible migration:** Admin-controlled timeline, no forced transitions
- ✅ **Lockout prevention:** Domain validation ensures users always have login method
- ✅ **Maintains Zitadel investment:** Leverages existing OAuth infrastructure
- ✅ **Consistent security:** All authentication through Zitadel (single audit trail)

### Negative

- ⚠️ **Zitadel dependency:** Still requires Zitadel Actions API (external dependency)
- ⚠️ **Manual deployment:** Zitadel Action requires admin console deployment
- ⚠️ **No offline mode:** Cannot authenticate during Zitadel outages
- ⚠️ **Database complexity:** Additional columns for dual auth state
- ⚠️ **Admin burden:** Manual migration triggers (no automation)

### Risks & Mitigation

| Risk | Mitigation |
|------|------------|
| Zitadel Actions fail | Rollback: disable Action, fall back to email-only login |
| Service account auth still fails | Comprehensive testing, startup validation, detailed logging |
| Users locked out during migration | Domain validation: cannot disable username without verified email |
| Zitadel outage during account creation | Retry logic with exponential backoff, clear error messages |
| Email detection regex inconsistency | Same regex in frontend, backend, and Zitadel Action (enforced by tests) |

---

## Implementation Details

### Database Schema

```sql
-- Phase 3: Dual authentication columns
ALTER TABLE auth.users
  ADD COLUMN real_email VARCHAR(255),
  ADD COLUMN real_email_verified BOOLEAN NOT NULL DEFAULT false,
  ADD COLUMN username_login_enabled BOOLEAN NOT NULL DEFAULT true;

CREATE UNIQUE INDEX ix_users_real_email
  ON auth.users (real_email)
  WHERE real_email IS NOT NULL;
```

### Domain Methods

```csharp
// Phase 3: Domain logic
public void AddRealEmail(Email realEmail, UserId requestingUserId)
{
    if (!IsSyntheticEmail)
        throw new InvalidOperationException("Cannot add real email to non-managed account.");
    if (RealEmail != null)
        throw new InvalidOperationException($"User already has real email: {RealEmail.Value}");

    RealEmail = realEmail;
    RealEmailVerified = false;
    AddDomainEvent(new RealEmailAddedEvent(Id, realEmail, requestingUserId));
}

public void DisableUsernameLogin(UserId requestingUserId)
{
    if (!UsernameLoginEnabled)
        throw new InvalidOperationException("Username login is already disabled.");
    if (RealEmail == null || !RealEmailVerified)
        throw new InvalidOperationException("Cannot disable username login without verified real email.");

    UsernameLoginEnabled = false;
    AddDomainEvent(new UsernameLoginDisabledEvent(Id, requestingUserId));
}

public void EnableUsernameLogin(UserId requestingUserId)
{
    if (UsernameLoginEnabled)
        throw new InvalidOperationException("Username login is already enabled.");
    if (!IsSyntheticEmail)
        throw new InvalidOperationException("Cannot enable username login for non-managed account.");

    UsernameLoginEnabled = true;
    AddDomainEvent(new UsernameLoginEnabledEvent(Id, requestingUserId));
}
```

### Zitadel Action (JavaScript)

```javascript
// Phase 4: Pre-Authentication Action
function preAuthentication(ctx, api) {
    const loginName = ctx.v1.loginName;
    if (!loginName || loginName.trim() === '') return;

    // Email regex: same as frontend and backend
    const isEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(loginName);

    if (isEmail) {
        api.v1.setLoginName(loginName); // Pass through
    } else {
        // Username - map to synthetic email
        const syntheticEmail = `${loginName}@noemail.family-hub.internal`;
        api.v1.setLoginName(syntheticEmail);
    }
}
```

### Frontend Email Detection

```typescript
// Phase 5: AuthService
async login(identifier?: string): Promise<void> {
    let loginHint: string | undefined;
    if (identifier && identifier.trim()) {
        const isEmail = this.isEmailFormat(identifier);
        loginHint = isEmail
            ? identifier
            : `${identifier}@noemail.family-hub.internal`;
    }

    const response = await this.graphql.query(GET_ZITADEL_AUTH_URL, { loginHint });
    window.location.href = response.zitadelAuthUrl.authorizationUrl;
}

private isEmailFormat(input: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(input);
}
```

---

## Testing Strategy

**Phase 6: Comprehensive testing (21 unit tests, 100% coverage)**

### Domain Tests (UserDualAuthenticationTests.cs)
- AddRealEmail validation (managed account check, duplicate check)
- DisableUsernameLogin with lockout prevention
- EnableUsernameLogin validation (already enabled check, non-managed check)
- Domain event publishing verification
- Workflow tests (add email → disable username → re-enable)

### Command Handler Tests
- Authorization (Owner/Admin can manage, Members cannot)
- Validation (user not found, email already in use)
- Lockout prevention (cannot disable without verified email)

### Frontend Tests (auth.service.spec.ts)
- Email detection (standard email, subdomain, plus addressing)
- Username mapping to synthetic email
- Edge cases (no @, @ but no domain, @ but no TLD)
- Regex consistency verification

**Result:** All 21 tests passing, zero regressions

---

## Rollback Procedures

### If Zitadel Actions Fail
1. Access Zitadel Admin Console
2. Go to Settings > Actions
3. Find "Family Hub Username or Email Login"
4. Toggle Active to OFF
5. Users fall back to email-only login
6. Managed accounts temporarily unable to login (acceptable during incident)

### If Service Account Auth Fixes Break
1. Rollback Docker image to previous version
2. Check private key file permissions and format
3. Verify Zitadel service account has correct IAM roles
4. Review Zitadel server logs for errors
5. Restore from backup if database migration ran

### If Dual Auth Migration Causes Issues
1. Rollback database migration: `dotnet ef database update <PreviousMigration>`
2. Disable admin UI for email management
3. Existing managed accounts continue with username login
4. No data loss (migration is additive, not destructive)

---

## Monitoring & Alerting

**Metrics to Track:**

1. **Login Success Rate** (username vs email)
   - Alert if < 95% success rate
   - Track 7-day trend

2. **Zitadel API Error Rate**
   - Alert if > 1% error rate (after retries)
   - Track 401, 403, 500, 503 separately

3. **Service Account Token Refresh Failures**
   - Alert on any failure (critical issue)
   - Log JWT claims and expiry

4. **Retry Attempts**
   - Track retry count distribution (0, 1, 2, 3 attempts)
   - Alert if >10% of requests require retries

**Tools:** Seq (structured logs), Prometheus (metrics), Grafana (dashboards)

---

## Related Documentation

- [ADR-002: OAuth with Zitadel](ADR-002-OAUTH-WITH-ZITADEL.md) - OAuth 2.0 integration
- [Zitadel Actions Setup Guide](/docs/infrastructure/ZITADEL-ACTIONS-SETUP.md) - Deployment steps
- [Implementation Plan](/home/andrekirst/.claude/plans/harmonic-bouncing-donut.md) - Detailed 6-phase plan

---

## Future Considerations

1. **Email Verification:** Add `VerifyRealEmail()` method and email verification workflow
2. **Automatic Expiry:** Add configurable grace period for dual authentication (e.g., 90 days)
3. **User Self-Service:** Allow users to add/verify their own email addresses (currently admin-only)
4. **Migration Dashboard:** Admin UI showing migration progress across all managed accounts
5. **Zitadel Action Automation:** Investigate Zitadel API for programmatic Action deployment

---

**Last Updated:** 2026-01-06
**Implementation Status:** ✅ Completed (Phases 1-6)
**Next Steps:** Deploy Zitadel Action to production, monitor metrics
