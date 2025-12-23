# Auth Module Schema - Visual Summary

**Quick Reference Guide**
**Version:** 1.0
**Date:** 2025-12-22

---

## Entity Relationship Diagram (ERD)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          AUTH MODULE SCHEMA                                  │
│                         PostgreSQL 16 + RLS                                  │
└─────────────────────────────────────────────────────────────────────────────┘

                    ┌──────────────────────────┐
                    │      auth.users          │
                    ├──────────────────────────┤
                    │ PK id (UUID)             │
                    │ UK email                 │
                    │    password_hash         │
                    │    email_verified        │
                    │    email_verified_at     │
                    │ UK zitadel_user_id       │
                    │    created_at            │
                    │    updated_at            │
                    │    deleted_at            │
                    │    last_login_at         │
                    └──────────────────────────┘
                             │         │
                             │         └──────────────────┐
                ┌────────────┘                            │
                │                                         │
                ▼                                         ▼
┌───────────────────────────┐            ┌─────────────────────────────┐
│ auth.email_verification_  │            │ auth.password_reset_tokens  │
│ tokens                    │            │                             │
├───────────────────────────┤            ├─────────────────────────────┤
│ PK id (UUID)              │            │ PK id (UUID)                │
│ FK user_id → users.id     │            │ FK user_id → users.id       │
│ UK token                  │            │ UK token                    │
│    expires_at             │            │    expires_at               │
│    created_at             │            │    created_at               │
│    used_at                │            │    used_at                  │
└───────────────────────────┘            └─────────────────────────────┘


                    ┌──────────────────────────┐
                    │    auth.families         │
                    ├──────────────────────────┤
                    │ PK id (UUID)             │
                    │    name                  │
                    │ FK owner_id → users.id   │
                    │    created_at            │
                    │    updated_at            │
                    │    deleted_at            │
                    └──────────────────────────┘
                             │         │
                ┌────────────┘         └────────────┐
                │                                   │
                ▼                                   ▼
┌───────────────────────────────────┐    ┌─────────────────────────┐
│   auth.user_families (junction)   │    │  auth.auth_audit_log    │
├───────────────────────────────────┤    ├─────────────────────────┤
│ PK id (UUID)                      │    │ PK id (UUID)            │
│ FK user_id → users.id             │    │ FK user_id → users.id   │
│ FK family_id → families.id        │    │    event_type           │
│ FK invited_by → users.id          │    │    event_data (JSONB)   │
│    role (enum)                    │    │    ip_address (INET)    │
│    is_active                      │    │    user_agent           │
│    joined_at                      │    │    success              │
│    invitation_accepted_at         │    │    failure_reason       │
│ UK (user_id, family_id)           │    │    created_at           │
└───────────────────────────────────┘    └─────────────────────────┘
```

---

## Table Summary

| Table | Rows (Est.) | Purpose | Key Features |
|-------|-------------|---------|--------------|
| **users** | 10K-1M | User identity & auth | UUID PK, soft deletes, RLS |
| **families** | 2K-100K | Family groups | Owner reference, soft deletes |
| **user_families** | 20K-500K | User-family membership | Roles, invitation tracking |
| **email_verification_tokens** | 100-10K | Email verification | Auto-expire, single-use |
| **password_reset_tokens** | 50-5K | Password reset | Short expiry (1-4 hrs) |
| **auth_audit_log** | 100K-10M | Security audit trail | JSONB data, time-series |

---

## Enumeration Types

### FamilyRole

```sql
CREATE TYPE family_role AS ENUM (
    'owner',    -- Full control, can delete family
    'admin',    -- Can manage members, edit settings
    'member',   -- Standard family member
    'child'     -- Limited permissions (future: parental controls)
);
```

**Role Hierarchy:**
```
owner > admin > member > child
```

**Permissions Matrix:**

| Action | Owner | Admin | Member | Child |
|--------|-------|-------|--------|-------|
| Delete family | ✓ | ✗ | ✗ | ✗ |
| Transfer ownership | ✓ | ✗ | ✗ | ✗ |
| Invite members | ✓ | ✓ | ✗ | ✗ |
| Remove members | ✓ | ✓ | ✗ | ✗ |
| Change member roles | ✓ | ✓ | ✗ | ✗ |
| Edit family name | ✓ | ✓ | ✗ | ✗ |
| View family data | ✓ | ✓ | ✓ | ✓ |
| Leave family | ✗ | ✓ | ✓ | ✓ |

### AuthEventType

```sql
-- Audit log event types
user_registered
user_logged_in
user_logged_out
email_verified
password_changed
password_reset_requested
password_reset_completed
login_failed
family_created
family_updated
family_ownership_transferred
family_deleted
family_member_invited
family_invitation_accepted
family_member_role_changed
family_member_deactivated
family_member_removed
```

---

## Data Flows

### User Registration Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                     USER REGISTRATION                            │
└─────────────────────────────────────────────────────────────────┘

1. User submits registration form
   ↓
2. Application validates input
   ↓
3. INSERT into auth.users
   - email: user@example.com
   - password_hash: $2a$12$...  (bcrypt)
   - email_verified: FALSE
   ↓
4. Trigger: auth.log_user_changes()
   - Inserts 'user_registered' event into auth_audit_log
   ↓
5. Application generates verification token
   ↓
6. INSERT into auth.email_verification_tokens
   - user_id: [new user UUID]
   - token: [32-byte random, base64]
   - expires_at: NOW() + 24 hours
   ↓
7. Send verification email
   - Link: https://app.familyhub.com/verify-email?token=[token]
   ↓
8. User clicks verification link
   ↓
9. Application validates token
   - Check: used_at IS NULL
   - Check: expires_at > NOW()
   ↓
10. UPDATE auth.users
    - SET email_verified = TRUE
    - SET email_verified_at = NOW()
    ↓
11. UPDATE auth.email_verification_tokens
    - SET used_at = NOW()
    ↓
12. Trigger: auth.log_user_changes()
    - Inserts 'email_verified' event into auth_audit_log
    ↓
13. Registration complete!
```

### Password Reset Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                     PASSWORD RESET                               │
└─────────────────────────────────────────────────────────────────┘

1. User clicks "Forgot Password"
   ↓
2. User enters email address
   ↓
3. Application finds user by email
   - SELECT id FROM auth.users WHERE email = ?
   ↓
4. INSERT into auth.password_reset_tokens
   - user_id: [user UUID]
   - token: [32-byte random, base64]
   - expires_at: NOW() + 1 hour
   ↓
5. Log audit event
   - INSERT into auth_audit_log (event_type: 'password_reset_requested')
   ↓
6. Send password reset email
   - Link: https://app.familyhub.com/reset-password?token=[token]
   ↓
7. User clicks reset link
   ↓
8. Application validates token
   - Check: used_at IS NULL
   - Check: expires_at > NOW()
   ↓
9. User submits new password
   ↓
10. Application validates password strength
    ↓
11. UPDATE auth.users
    - SET password_hash = [new bcrypt hash]
    - SET updated_at = NOW()
    ↓
12. UPDATE auth.password_reset_tokens
    - SET used_at = NOW()
    ↓
13. Trigger: auth.log_user_changes()
    - Inserts 'password_changed' event into auth_audit_log
    ↓
14. Password reset complete!
```

### Family Creation & Invitation Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                  FAMILY CREATION & INVITATION                    │
└─────────────────────────────────────────────────────────────────┘

1. User creates new family
   ↓
2. INSERT into auth.families
   - name: "Smith Family"
   - owner_id: [user UUID]
   ↓
3. Trigger: auth.validate_family_owner_membership()
   - Automatically inserts into auth.user_families:
     * user_id: [owner UUID]
     * family_id: [new family UUID]
     * role: 'owner'
     * is_active: TRUE
     * invitation_accepted_at: NOW()
   ↓
4. Trigger: auth.log_family_changes()
   - Inserts 'family_created' event into auth_audit_log
   ↓
5. Owner invites family member
   ↓
6. Application validates invitee email
   - Must be registered user OR send registration invitation
   ↓
7. INSERT into auth.user_families
   - user_id: [invitee UUID]
   - family_id: [family UUID]
   - role: 'member'
   - invited_by: [owner UUID]
   - invitation_accepted_at: NULL  (pending)
   ↓
8. Trigger: auth.log_membership_changes()
   - Inserts 'family_member_invited' event into auth_audit_log
   ↓
9. Send invitation email to invitee
   - Link: https://app.familyhub.com/invitations/[family_id]
   ↓
10. Invitee clicks invitation link
    ↓
11. UPDATE auth.user_families
    - SET invitation_accepted_at = NOW()
    - WHERE user_id = [invitee] AND family_id = [family]
    ↓
12. Trigger: auth.log_membership_changes()
    - Inserts 'family_invitation_accepted' event
    ↓
13. Invitation accepted!
```

---

## Row-Level Security (RLS) Examples

### Setting Current User Session

Before executing queries, the application must set the current user:

```sql
-- Set session variable (once per request)
SET app.current_user_id = '00000000-0000-0000-0000-000000000001';

-- All subsequent queries use RLS policies based on this user
```

### Query Examples with RLS

**Example 1: User viewing their own profile**

```sql
-- Application code (user viewing their profile)
SET app.current_user_id = 'user-uuid-123';

-- This query works because RLS policy allows users to see their own record
SELECT email, email_verified, created_at
FROM auth.users
WHERE id = 'user-uuid-123';

-- ✓ RLS Policy: users_select_own (USING id = current_user_id())
```

**Example 2: User trying to view another user's profile**

```sql
SET app.current_user_id = 'user-uuid-123';

-- This query returns ZERO rows due to RLS
SELECT email, email_verified, created_at
FROM auth.users
WHERE id = 'different-user-uuid-456';

-- ✗ RLS Policy blocks: user can only see their own record
```

**Example 3: Viewing family members**

```sql
SET app.current_user_id = 'user-uuid-123';

-- This query returns only families where user is a member
SELECT f.id, f.name, uf.role
FROM auth.families f
JOIN auth.user_families uf ON f.id = uf.family_id
WHERE uf.user_id = 'user-uuid-123'
AND uf.is_active = TRUE;

-- ✓ RLS Policy: families_select_member (checks user_families membership)
```

**Example 4: Service account bypassing RLS**

```sql
-- Service account connects as 'family_hub_service' role
-- RLS is BYPASSED for this role

-- Can query any user data for application logic
SELECT * FROM auth.users WHERE email = 'any@user.com';

-- ✓ Service role has BYPASSRLS privilege
```

---

## Index Strategy

### Primary Indexes (Automatically Created)

```sql
-- Primary key indexes (B-tree, unique)
auth.users_pkey ON users(id)
auth.families_pkey ON families(id)
auth.user_families_pkey ON user_families(id)
auth.email_verification_tokens_pkey ON email_verification_tokens(id)
auth.password_reset_tokens_pkey ON password_reset_tokens(id)
auth.auth_audit_log_pkey ON auth_audit_log(id)
```

### Secondary Indexes (Query Optimization)

**User Lookups:**
```sql
-- Email lookup (login)
idx_users_email ON users(email) WHERE deleted_at IS NULL
-- Usage: Login by email, user search

-- OAuth mapping
idx_users_zitadel_user_id ON users(zitadel_user_id) WHERE deleted_at IS NULL
-- Usage: OAuth 2.0 authentication
```

**Family Queries:**
```sql
-- Find families by owner
idx_families_owner_id ON families(owner_id) WHERE deleted_at IS NULL
-- Usage: "My families" query

-- Family memberships by user
idx_user_families_user_id ON user_families(user_id) WHERE is_active = TRUE
-- Usage: Get user's family memberships

-- Family members by family
idx_user_families_family_id ON user_families(family_id) WHERE is_active = TRUE
-- Usage: List all members of a family

-- Role-based queries
idx_user_families_role ON user_families(family_id, role)
-- Usage: Find admins, count members by role
```

**Token Lookups:**
```sql
-- Active token lookup (partial index)
idx_email_verification_tokens_token ON email_verification_tokens(token)
    WHERE used_at IS NULL AND expires_at > CURRENT_TIMESTAMP
-- Usage: Verify email with token

idx_password_reset_tokens_token ON password_reset_tokens(token)
    WHERE used_at IS NULL AND expires_at > CURRENT_TIMESTAMP
-- Usage: Reset password with token
```

**Audit Log Queries:**
```sql
-- User activity timeline
idx_auth_audit_log_user_id ON auth_audit_log(user_id, created_at DESC)
-- Usage: Recent activity for user

-- Event type analysis
idx_auth_audit_log_event_type ON auth_audit_log(event_type, created_at DESC)
-- Usage: All login events, all failed attempts

-- Security monitoring
idx_auth_audit_log_ip_address ON auth_audit_log(ip_address, created_at DESC)
-- Usage: Track activity by IP, detect suspicious patterns

-- Failed login attempts
idx_auth_audit_log_success ON auth_audit_log(success, created_at DESC)
    WHERE success = FALSE
-- Usage: Brute force detection

-- JSONB queries
idx_auth_audit_log_event_data ON auth_audit_log USING gin(event_data)
-- Usage: Query event_data fields (e.g., find events for specific family_id)
```

---

## Performance Characteristics

### Query Performance Targets

| Query Type | Target | Index Used |
|------------|--------|------------|
| Login by email | < 5ms | idx_users_email |
| Get user's families | < 10ms | idx_user_families_user_id |
| List family members | < 15ms | idx_user_families_family_id |
| Verify email token | < 5ms | idx_email_verification_tokens_token |
| Recent audit logs | < 20ms | idx_auth_audit_log_user_id |

### Table Size Estimates (at scale)

**Assumptions:**
- 10,000 users
- 2,000 families
- 20,000 memberships
- 1M audit log entries/year

**Storage Estimates:**

```
auth.users:                      ~1 MB  (100 bytes/row × 10K)
auth.families:                   ~200 KB (100 bytes/row × 2K)
auth.user_families:              ~2 MB  (100 bytes/row × 20K)
auth.email_verification_tokens:  ~500 KB (transient, auto-cleanup)
auth.password_reset_tokens:      ~200 KB (transient, auto-cleanup)
auth.auth_audit_log:             ~500 MB (500 bytes/row × 1M)
-------------------------------------------------------------------
Total (without indexes):         ~504 MB
Indexes (estimated 50% overhead): ~250 MB
-------------------------------------------------------------------
Grand Total:                     ~754 MB
```

---

## Security Checklist

### Database Security

- [x] Row-Level Security (RLS) enabled on all tables
- [x] RLS policies enforce multi-tenancy isolation
- [x] Service role with BYPASSRLS for application
- [x] Passwords hashed with bcrypt (cost factor 12)
- [x] Tokens cryptographically secure (32 bytes random)
- [x] Token expiration enforced at database level
- [x] Soft deletes for GDPR compliance
- [x] Audit logging for all auth events
- [x] Email format validation (CHECK constraint)
- [x] Foreign key constraints with appropriate ON DELETE

### Application Security

- [ ] Implement rate limiting for login attempts
- [ ] Implement CAPTCHA for repeated failed logins
- [ ] Email verification required before full access
- [ ] Password strength validation (12+ chars, complexity)
- [ ] Account lockout after N failed attempts
- [ ] Multi-factor authentication (MFA) - Phase 2
- [ ] Session management with timeout
- [ ] CSRF protection on all mutations
- [ ] SQL injection prevention (parameterized queries)
- [ ] XSS prevention (sanitize all inputs)

---

## Quick Reference: Common Queries

### Authentication

```sql
-- Authenticate user
SELECT id, email, email_verified, password_hash
FROM auth.users
WHERE email = $1
AND deleted_at IS NULL;

-- Update last login
UPDATE auth.users
SET last_login_at = CURRENT_TIMESTAMP
WHERE id = $1;
```

### Family Management

```sql
-- Get user's families with role
SELECT f.id, f.name, uf.role, uf.is_active
FROM auth.families f
JOIN auth.user_families uf ON f.id = uf.family_id
WHERE uf.user_id = $1
AND f.deleted_at IS NULL
ORDER BY uf.joined_at DESC;

-- Check if user is family admin/owner
SELECT EXISTS (
    SELECT 1 FROM auth.user_families
    WHERE user_id = $1
    AND family_id = $2
    AND role IN ('owner', 'admin')
    AND is_active = TRUE
) AS is_admin;
```

### Security Monitoring

```sql
-- Detect brute force attempts
SELECT
    ip_address,
    COUNT(*) as failed_attempts,
    MAX(created_at) as last_attempt
FROM auth.auth_audit_log
WHERE event_type = 'login_failed'
AND created_at > CURRENT_TIMESTAMP - INTERVAL '1 hour'
GROUP BY ip_address
HAVING COUNT(*) >= 5;

-- Recent suspicious activity
SELECT
    user_id,
    event_type,
    ip_address,
    created_at
FROM auth.auth_audit_log
WHERE success = FALSE
AND created_at > CURRENT_TIMESTAMP - INTERVAL '24 hours'
ORDER BY created_at DESC;
```

---

**Document Version:** 1.0
**Last Updated:** 2025-12-22
**Database:** PostgreSQL 16
**Schema:** auth (v1.0)
