# Auth Module Database Schema Design

**⚠️ IMPORTANT: This is REFERENCE DOCUMENTATION. Actual schema is implemented via EF Core Code-First migrations.**

**Version:** 1.0
**Date:** 2025-12-22
**Status:** Reference Documentation (Implementation uses EF Core)
**Database:** PostgreSQL 16
**Schema:** `auth`

---

## Executive Summary

This document provides the complete database schema design for the Auth Module, implementing user registration, authentication, family group management, and OAuth 2.0 integration with Zitadel. The schema follows Domain-Driven Design (DDD) principles with User and FamilyGroup as aggregate roots.

**Implementation Note:** This design is implemented using **EF Core Code-First migrations**, not the SQL scripts shown in this document. The SQL scripts in `/database/docs/reference/sql-design/` serve as reference documentation for understanding the intended schema structure, Row-Level Security policies, triggers, and constraints. See `/database/docs/MIGRATION_STRATEGY.md` for the actual implementation approach.

**Key Features:**
- User registration with email verification
- Password reset workflow with secure tokens
- Family group management with role-based access
- Zitadel OAuth 2.0 external identity mapping
- Row-Level Security (RLS) for multi-tenancy
- Audit trail for authentication events
- Soft deletes for data retention compliance

---

## Table of Contents

1. [Schema Overview](#schema-overview)
2. [Domain Model](#domain-model)
3. [Table Definitions](#table-definitions)
4. [Indexes and Constraints](#indexes-and-constraints)
5. [Row-Level Security Policies](#row-level-security-policies)
6. [Migration Scripts](#migration-scripts)
7. [Sample Queries](#sample-queries)
8. [Performance Considerations](#performance-considerations)
9. [Security Considerations](#security-considerations)
10. [Integration Points](#integration-points)

---

## Schema Overview

### Entity Relationship Diagram (ERD)

```
┌────────────────────────────────────────────────────────────────────────┐
│                        Auth Schema - PostgreSQL                        │
└────────────────────────────────────────────────────────────────────────┘

┌─────────────────────┐           ┌──────────────────────┐
│     users           │           │   families           │
├─────────────────────┤           ├──────────────────────┤
│ id (PK)             │           │ id (PK)              │
│ email (UNIQUE)      │           │ name                 │
│ password_hash       │           │ owner_id (FK→users)  │
│ email_verified      │           │ created_at           │
│ email_verified_at   │           │ updated_at           │
│ zitadel_user_id     │           │ deleted_at           │
│ created_at          │           └──────────────────────┘
│ updated_at          │                      │
│ deleted_at          │                      │
│ last_login_at       │                      │
└─────────────────────┘                      │
         │                                   │
         │                                   │
         │          ┌──────────────────────────────────┐
         └──────────│   user_families (junction)       │
                    ├──────────────────────────────────┤
                    │ id (PK)                          │
                    │ user_id (FK→users)               │
                    │ family_id (FK→families)          │
                    │ role                             │
                    │ joined_at                        │
                    │ is_active                        │
                    │ invited_by (FK→users)            │
                    │ invitation_accepted_at           │
                    └──────────────────────────────────┘

┌─────────────────────────────┐   ┌──────────────────────────────┐
│  email_verification_tokens  │   │  password_reset_tokens       │
├─────────────────────────────┤   ├──────────────────────────────┤
│ id (PK)                     │   │ id (PK)                      │
│ user_id (FK→users)          │   │ user_id (FK→users)           │
│ token (UNIQUE)              │   │ token (UNIQUE)               │
│ expires_at                  │   │ expires_at                   │
│ created_at                  │   │ created_at                   │
│ used_at                     │   │ used_at                      │
└─────────────────────────────┘   └──────────────────────────────┘

┌──────────────────────────────────────┐
│  auth_audit_log                      │
├──────────────────────────────────────┤
│ id (PK)                              │
│ user_id (FK→users, nullable)         │
│ event_type                           │
│ event_data (JSONB)                   │
│ ip_address                           │
│ user_agent                           │
│ success                              │
│ failure_reason                       │
│ created_at                           │
└──────────────────────────────────────┘
```

### Aggregate Roots

1. **User** - Manages user identity, authentication, and profile
2. **Family** - Manages family groups and membership

### Supporting Entities

- **UserFamily** - Junction entity managing many-to-many relationships with roles
- **EmailVerificationToken** - Value object for email verification workflow
- **PasswordResetToken** - Value object for password reset workflow
- **AuthAuditLog** - Event sourcing for security auditing

---

## Domain Model

### User Aggregate

```csharp
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }
    public string ZitadelUserId { get; private set; }  // External OAuth ID
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Domain methods
    public void VerifyEmail();
    public void UpdatePassword(string newPasswordHash);
    public void RecordLogin();
    public void SoftDelete();
}
```

### Family Aggregate

```csharp
public class Family
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public List<UserFamily> Members { get; private set; }

    // Domain methods
    public void AddMember(Guid userId, FamilyRole role, Guid invitedBy);
    public void RemoveMember(Guid userId);
    public void UpdateMemberRole(Guid userId, FamilyRole newRole);
    public void TransferOwnership(Guid newOwnerId);
}
```

### Value Objects

```csharp
public enum FamilyRole
{
    Owner = 1,
    Admin = 2,
    Member = 3,
    Child = 4
}

public enum AuthEventType
{
    UserRegistered = 1,
    UserLoggedIn = 2,
    UserLoggedOut = 3,
    PasswordChanged = 4,
    PasswordResetRequested = 5,
    PasswordResetCompleted = 6,
    EmailVerified = 7,
    LoginFailed = 8,
    FamilyCreated = 9,
    MemberInvited = 10,
    MemberJoined = 11,
    MemberRemoved = 12
}
```

---

## Table Definitions

### 1. users

Core user identity and authentication table.

```sql
CREATE TABLE auth.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Authentication
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,

    -- Email verification
    email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    email_verified_at TIMESTAMPTZ,

    -- External OAuth integration
    zitadel_user_id VARCHAR(255) UNIQUE,

    -- Audit columns
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,

    -- Session tracking
    last_login_at TIMESTAMPTZ,

    -- Constraints
    CONSTRAINT users_email_check CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT users_password_hash_check CHECK (length(password_hash) >= 60),  -- bcrypt minimum
    CONSTRAINT users_email_verified_consistency CHECK (
        (email_verified = TRUE AND email_verified_at IS NOT NULL) OR
        (email_verified = FALSE)
    )
);

-- Indexes
CREATE INDEX idx_users_email ON auth.users(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_zitadel_user_id ON auth.users(zitadel_user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_created_at ON auth.users(created_at);
CREATE INDEX idx_users_deleted_at ON auth.users(deleted_at) WHERE deleted_at IS NOT NULL;

-- Comments
COMMENT ON TABLE auth.users IS 'Core user identity and authentication';
COMMENT ON COLUMN auth.users.password_hash IS 'bcrypt hashed password (60 chars)';
COMMENT ON COLUMN auth.users.zitadel_user_id IS 'External Zitadel OAuth 2.0 user identifier';
COMMENT ON COLUMN auth.users.deleted_at IS 'Soft delete timestamp for GDPR compliance';
```

### 2. families

Family group management table.

```sql
CREATE TABLE auth.families (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Family information
    name VARCHAR(255) NOT NULL,
    owner_id UUID NOT NULL,

    -- Audit columns
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,

    -- Constraints
    CONSTRAINT families_owner_id_fkey FOREIGN KEY (owner_id)
        REFERENCES auth.users(id)
        ON DELETE RESTRICT,
    CONSTRAINT families_name_check CHECK (length(trim(name)) >= 1)
);

-- Indexes
CREATE INDEX idx_families_owner_id ON auth.families(owner_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_families_created_at ON auth.families(created_at);
CREATE INDEX idx_families_deleted_at ON auth.families(deleted_at) WHERE deleted_at IS NOT NULL;

-- Comments
COMMENT ON TABLE auth.families IS 'Family groups for organizing users';
COMMENT ON COLUMN auth.families.owner_id IS 'User who created and owns the family group';
```

### 3. user_families (Junction Table)

Many-to-many relationship between users and families with role information.

```sql
CREATE TABLE auth.user_families (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Relationships
    user_id UUID NOT NULL,
    family_id UUID NOT NULL,

    -- Role and status
    role VARCHAR(50) NOT NULL DEFAULT 'member',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    -- Invitation tracking
    invited_by UUID,
    joined_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    invitation_accepted_at TIMESTAMPTZ,

    -- Constraints
    CONSTRAINT user_families_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES auth.users(id)
        ON DELETE CASCADE,
    CONSTRAINT user_families_family_id_fkey FOREIGN KEY (family_id)
        REFERENCES auth.families(id)
        ON DELETE CASCADE,
    CONSTRAINT user_families_invited_by_fkey FOREIGN KEY (invited_by)
        REFERENCES auth.users(id)
        ON DELETE SET NULL,
    CONSTRAINT user_families_role_check CHECK (role IN ('owner', 'admin', 'member', 'child')),
    CONSTRAINT user_families_unique_membership UNIQUE (user_id, family_id)
);

-- Indexes
CREATE INDEX idx_user_families_user_id ON auth.user_families(user_id) WHERE is_active = TRUE;
CREATE INDEX idx_user_families_family_id ON auth.user_families(family_id) WHERE is_active = TRUE;
CREATE INDEX idx_user_families_role ON auth.user_families(family_id, role);
CREATE INDEX idx_user_families_invited_by ON auth.user_families(invited_by);

-- Comments
COMMENT ON TABLE auth.user_families IS 'Many-to-many relationship between users and families with roles';
COMMENT ON COLUMN auth.user_families.role IS 'Family role: owner, admin, member, child';
COMMENT ON COLUMN auth.user_families.is_active IS 'Whether membership is currently active';
COMMENT ON COLUMN auth.user_families.invited_by IS 'User who sent the family invitation';
```

### 4. email_verification_tokens

Email verification workflow tokens.

```sql
CREATE TABLE auth.email_verification_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- User reference
    user_id UUID NOT NULL,

    -- Token data
    token VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,

    -- Usage tracking
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    used_at TIMESTAMPTZ,

    -- Constraints
    CONSTRAINT email_verification_tokens_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES auth.users(id)
        ON DELETE CASCADE,
    CONSTRAINT email_verification_tokens_expires_check CHECK (expires_at > created_at),
    CONSTRAINT email_verification_tokens_used_check CHECK (used_at IS NULL OR used_at >= created_at)
);

-- Indexes
CREATE INDEX idx_email_verification_tokens_token ON auth.email_verification_tokens(token)
    WHERE used_at IS NULL AND expires_at > CURRENT_TIMESTAMP;
CREATE INDEX idx_email_verification_tokens_user_id ON auth.email_verification_tokens(user_id);
CREATE INDEX idx_email_verification_tokens_expires_at ON auth.email_verification_tokens(expires_at);

-- Comments
COMMENT ON TABLE auth.email_verification_tokens IS 'Tokens for email verification workflow';
COMMENT ON COLUMN auth.email_verification_tokens.token IS 'Cryptographically secure random token (URL-safe)';
COMMENT ON COLUMN auth.email_verification_tokens.expires_at IS 'Token expiration (typically 24-48 hours)';
```

### 5. password_reset_tokens

Password reset workflow tokens.

```sql
CREATE TABLE auth.password_reset_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- User reference
    user_id UUID NOT NULL,

    -- Token data
    token VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,

    -- Usage tracking
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    used_at TIMESTAMPTZ,

    -- Constraints
    CONSTRAINT password_reset_tokens_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES auth.users(id)
        ON DELETE CASCADE,
    CONSTRAINT password_reset_tokens_expires_check CHECK (expires_at > created_at),
    CONSTRAINT password_reset_tokens_used_check CHECK (used_at IS NULL OR used_at >= created_at)
);

-- Indexes
CREATE INDEX idx_password_reset_tokens_token ON auth.password_reset_tokens(token)
    WHERE used_at IS NULL AND expires_at > CURRENT_TIMESTAMP;
CREATE INDEX idx_password_reset_tokens_user_id ON auth.password_reset_tokens(user_id);
CREATE INDEX idx_password_reset_tokens_expires_at ON auth.password_reset_tokens(expires_at);

-- Comments
COMMENT ON TABLE auth.password_reset_tokens IS 'Tokens for password reset workflow';
COMMENT ON COLUMN auth.password_reset_tokens.token IS 'Cryptographically secure random token (URL-safe)';
COMMENT ON COLUMN auth.password_reset_tokens.expires_at IS 'Token expiration (typically 1-4 hours)';
```

### 6. auth_audit_log

Authentication and authorization event audit log.

```sql
CREATE TABLE auth.auth_audit_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- User reference (nullable for failed login attempts)
    user_id UUID,

    -- Event information
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL DEFAULT '{}',

    -- Request metadata
    ip_address INET,
    user_agent TEXT,

    -- Outcome
    success BOOLEAN NOT NULL DEFAULT TRUE,
    failure_reason TEXT,

    -- Timestamp
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CONSTRAINT auth_audit_log_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES auth.users(id)
        ON DELETE SET NULL,
    CONSTRAINT auth_audit_log_failure_check CHECK (
        (success = TRUE AND failure_reason IS NULL) OR
        (success = FALSE AND failure_reason IS NOT NULL)
    )
);

-- Indexes
CREATE INDEX idx_auth_audit_log_user_id ON auth.auth_audit_log(user_id, created_at DESC);
CREATE INDEX idx_auth_audit_log_event_type ON auth.auth_audit_log(event_type, created_at DESC);
CREATE INDEX idx_auth_audit_log_created_at ON auth.auth_audit_log(created_at DESC);
CREATE INDEX idx_auth_audit_log_ip_address ON auth.auth_audit_log(ip_address, created_at DESC);
CREATE INDEX idx_auth_audit_log_success ON auth.auth_audit_log(success, created_at DESC) WHERE success = FALSE;

-- JSONB index for event_data queries
CREATE INDEX idx_auth_audit_log_event_data ON auth.auth_audit_log USING gin(event_data);

-- Comments
COMMENT ON TABLE auth.auth_audit_log IS 'Audit trail for all authentication and authorization events';
COMMENT ON COLUMN auth.auth_audit_log.event_type IS 'Type of auth event (login, logout, password_change, etc.)';
COMMENT ON COLUMN auth.auth_audit_log.event_data IS 'Additional event-specific data in JSON format';
COMMENT ON COLUMN auth.auth_audit_log.user_id IS 'User associated with event (nullable for failed login attempts)';
```

---

## Indexes and Constraints

### Primary Keys

All tables use UUID primary keys generated with `gen_random_uuid()` for:
- Distributed system compatibility
- Non-sequential IDs (security)
- Global uniqueness across services

### Unique Constraints

- `users.email` - Prevent duplicate email registrations
- `users.zitadel_user_id` - One-to-one mapping with external OAuth provider
- `email_verification_tokens.token` - Prevent token collisions
- `password_reset_tokens.token` - Prevent token collisions
- `user_families(user_id, family_id)` - Prevent duplicate memberships

### Foreign Keys

All foreign keys use appropriate `ON DELETE` actions:
- `CASCADE` - For dependent entities (tokens, user_families)
- `RESTRICT` - For ownership references (family owner)
- `SET NULL` - For optional references (invited_by, audit logs)

### Check Constraints

- Email format validation (regex)
- Password hash minimum length (bcrypt = 60 chars)
- Role enum validation
- Timestamp consistency checks
- Success/failure logical consistency

### Performance Indexes

Composite indexes optimized for common query patterns:
- User lookups by email (WHERE deleted_at IS NULL)
- Family member queries (family_id + role)
- Audit log time-series queries (user_id + created_at DESC)
- Active token lookups (used_at IS NULL AND expires_at > NOW())

---

## Row-Level Security Policies

PostgreSQL Row-Level Security (RLS) enforces multi-tenancy at the database level.

### Enable RLS on Tables

```sql
ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.families ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.user_families ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.email_verification_tokens ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.password_reset_tokens ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.auth_audit_log ENABLE ROW LEVEL SECURITY;
```

### RLS Helper Function

```sql
-- Function to get current authenticated user ID from session
CREATE OR REPLACE FUNCTION auth.current_user_id()
RETURNS UUID AS $$
BEGIN
    RETURN current_setting('app.current_user_id', true)::UUID;
EXCEPTION
    WHEN OTHERS THEN
        RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE;

COMMENT ON FUNCTION auth.current_user_id() IS 'Returns UUID of currently authenticated user from session variable';
```

### RLS Policies

```sql
-- Users: Can only see their own record
CREATE POLICY users_select_own ON auth.users
    FOR SELECT
    USING (id = auth.current_user_id());

-- Users: Can only update their own record
CREATE POLICY users_update_own ON auth.users
    FOR UPDATE
    USING (id = auth.current_user_id());

-- Families: Can see families they are members of
CREATE POLICY families_select_member ON auth.families
    FOR SELECT
    USING (
        id IN (
            SELECT family_id
            FROM auth.user_families
            WHERE user_id = auth.current_user_id()
            AND is_active = TRUE
        )
    );

-- Families: Only owner can update
CREATE POLICY families_update_owner ON auth.families
    FOR UPDATE
    USING (owner_id = auth.current_user_id());

-- Families: Only owner can delete
CREATE POLICY families_delete_owner ON auth.families
    FOR DELETE
    USING (owner_id = auth.current_user_id());

-- UserFamilies: Can see memberships of families they belong to
CREATE POLICY user_families_select_member ON auth.user_families
    FOR SELECT
    USING (
        family_id IN (
            SELECT family_id
            FROM auth.user_families
            WHERE user_id = auth.current_user_id()
            AND is_active = TRUE
        )
    );

-- UserFamilies: Can only update own membership
CREATE POLICY user_families_update_own ON auth.user_families
    FOR UPDATE
    USING (user_id = auth.current_user_id());

-- EmailVerificationTokens: Can only see own tokens
CREATE POLICY email_verification_tokens_select_own ON auth.email_verification_tokens
    FOR SELECT
    USING (user_id = auth.current_user_id());

-- PasswordResetTokens: Can only see own tokens
CREATE POLICY password_reset_tokens_select_own ON auth.password_reset_tokens
    FOR SELECT
    USING (user_id = auth.current_user_id());

-- AuthAuditLog: Can only see own audit entries
CREATE POLICY auth_audit_log_select_own ON auth.auth_audit_log
    FOR SELECT
    USING (user_id = auth.current_user_id());
```

### Bypassing RLS for Service Accounts

```sql
-- Grant service role to bypass RLS
CREATE ROLE family_hub_service;
ALTER ROLE family_hub_service BYPASSRLS;
```

---

## Migration Scripts

Migration scripts are located in `/database/migrations/auth/`.

See:
- `001_create_auth_schema.sql` - Schema and tables
- `002_create_rls_policies.sql` - Row-Level Security
- `003_create_triggers.sql` - Triggers and functions
- `004_seed_data.sql` - Test data (development only)

---

## Sample Queries

### User Registration Workflow

```sql
-- 1. Create new user
INSERT INTO auth.users (email, password_hash, email_verified)
VALUES ('user@example.com', '$2a$12$...bcrypt_hash...', FALSE)
RETURNING id;

-- 2. Generate email verification token
INSERT INTO auth.email_verification_tokens (user_id, token, expires_at)
VALUES (
    'user-uuid',
    encode(gen_random_bytes(32), 'base64'),
    CURRENT_TIMESTAMP + INTERVAL '24 hours'
)
RETURNING token;

-- 3. Verify email
UPDATE auth.users
SET email_verified = TRUE, email_verified_at = CURRENT_TIMESTAMP
WHERE id = (
    SELECT user_id FROM auth.email_verification_tokens
    WHERE token = 'verification-token'
    AND used_at IS NULL
    AND expires_at > CURRENT_TIMESTAMP
);

UPDATE auth.email_verification_tokens
SET used_at = CURRENT_TIMESTAMP
WHERE token = 'verification-token';
```

### Password Reset Workflow

```sql
-- 1. Request password reset
INSERT INTO auth.password_reset_tokens (user_id, token, expires_at)
VALUES (
    (SELECT id FROM auth.users WHERE email = 'user@example.com'),
    encode(gen_random_bytes(32), 'base64'),
    CURRENT_TIMESTAMP + INTERVAL '1 hour'
)
RETURNING token;

-- 2. Reset password
UPDATE auth.users
SET password_hash = '$2a$12$...new_bcrypt_hash...', updated_at = CURRENT_TIMESTAMP
WHERE id = (
    SELECT user_id FROM auth.password_reset_tokens
    WHERE token = 'reset-token'
    AND used_at IS NULL
    AND expires_at > CURRENT_TIMESTAMP
);

UPDATE auth.password_reset_tokens
SET used_at = CURRENT_TIMESTAMP
WHERE token = 'reset-token';
```

### Family Group Management

```sql
-- 1. Create family
INSERT INTO auth.families (name, owner_id)
VALUES ('Smith Family', 'owner-user-uuid')
RETURNING id;

-- 2. Add owner as first member
INSERT INTO auth.user_families (user_id, family_id, role, invitation_accepted_at)
VALUES ('owner-user-uuid', 'family-uuid', 'owner', CURRENT_TIMESTAMP);

-- 3. Invite family member
INSERT INTO auth.user_families (user_id, family_id, role, invited_by)
VALUES ('invited-user-uuid', 'family-uuid', 'member', 'owner-user-uuid');

-- 4. Accept invitation
UPDATE auth.user_families
SET invitation_accepted_at = CURRENT_TIMESTAMP
WHERE user_id = 'invited-user-uuid' AND family_id = 'family-uuid';

-- 5. Get all family members
SELECT u.id, u.email, uf.role, uf.joined_at
FROM auth.users u
JOIN auth.user_families uf ON u.id = uf.user_id
WHERE uf.family_id = 'family-uuid'
AND uf.is_active = TRUE
AND u.deleted_at IS NULL
ORDER BY uf.joined_at;
```

### Authentication Queries

```sql
-- Login user
UPDATE auth.users
SET last_login_at = CURRENT_TIMESTAMP
WHERE email = 'user@example.com'
AND deleted_at IS NULL
RETURNING id, email, email_verified;

-- Get user's families
SELECT f.id, f.name, uf.role
FROM auth.families f
JOIN auth.user_families uf ON f.id = uf.family_id
WHERE uf.user_id = 'user-uuid'
AND uf.is_active = TRUE
AND f.deleted_at IS NULL;

-- Check if user is family admin or owner
SELECT EXISTS (
    SELECT 1 FROM auth.user_families
    WHERE user_id = 'user-uuid'
    AND family_id = 'family-uuid'
    AND role IN ('owner', 'admin')
    AND is_active = TRUE
) AS is_admin;
```

### Audit Log Queries

```sql
-- Record successful login
INSERT INTO auth.auth_audit_log (user_id, event_type, ip_address, user_agent, success)
VALUES (
    'user-uuid',
    'user_logged_in',
    '192.168.1.100'::INET,
    'Mozilla/5.0...',
    TRUE
);

-- Record failed login attempt
INSERT INTO auth.auth_audit_log (event_type, event_data, ip_address, success, failure_reason)
VALUES (
    'login_failed',
    jsonb_build_object('email', 'attempted-email@example.com'),
    '192.168.1.100'::INET,
    FALSE,
    'Invalid credentials'
);

-- Get recent login attempts for user
SELECT event_type, ip_address, success, created_at
FROM auth.auth_audit_log
WHERE user_id = 'user-uuid'
AND event_type IN ('user_logged_in', 'login_failed')
ORDER BY created_at DESC
LIMIT 10;

-- Detect suspicious login patterns (multiple failures)
SELECT ip_address, COUNT(*) as failed_attempts, MAX(created_at) as last_attempt
FROM auth.auth_audit_log
WHERE event_type = 'login_failed'
AND success = FALSE
AND created_at > CURRENT_TIMESTAMP - INTERVAL '1 hour'
GROUP BY ip_address
HAVING COUNT(*) >= 5;
```

---

## Performance Considerations

### Query Optimization

1. **Email Lookups**
   - Partial index on `users.email WHERE deleted_at IS NULL`
   - Reduces index size and improves query performance

2. **Active Token Lookups**
   - Partial indexes on tokens WHERE `used_at IS NULL AND expires_at > NOW()`
   - Automatically prunes expired/used tokens from index

3. **Family Member Queries**
   - Composite index on `(family_id, role)` for role-based queries
   - Partial index WHERE `is_active = TRUE` for active memberships

4. **Audit Log Time-Series**
   - B-tree index on `created_at DESC` for recent entries
   - Consider partitioning by month for large datasets (>10M rows)

### Connection Pooling

Recommended PgBouncer configuration:
```ini
pool_mode = transaction
max_client_conn = 1000
default_pool_size = 25
min_pool_size = 5
reserve_pool_size = 5
```

### Statistics and Vacuuming

```sql
-- Update statistics regularly
ANALYZE auth.users;
ANALYZE auth.families;
ANALYZE auth.user_families;
ANALYZE auth.auth_audit_log;

-- Autovacuum settings for high-churn tables
ALTER TABLE auth.email_verification_tokens SET (
    autovacuum_vacuum_scale_factor = 0.05,
    autovacuum_analyze_scale_factor = 0.05
);

ALTER TABLE auth.password_reset_tokens SET (
    autovacuum_vacuum_scale_factor = 0.05,
    autovacuum_analyze_scale_factor = 0.05
);
```

### Partitioning Strategy (Future)

For audit logs exceeding 10M rows, consider time-based partitioning:

```sql
-- Create partitioned table (PostgreSQL 11+)
CREATE TABLE auth.auth_audit_log_partitioned (
    LIKE auth.auth_audit_log INCLUDING ALL
) PARTITION BY RANGE (created_at);

-- Create monthly partitions
CREATE TABLE auth.auth_audit_log_2025_01
    PARTITION OF auth.auth_audit_log_partitioned
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');
```

---

## Security Considerations

### Password Security

1. **Hashing Algorithm**: bcrypt with cost factor 12
2. **Minimum Length**: 12 characters (enforced at application layer)
3. **Complexity Requirements**: Application-layer validation
4. **Storage**: Never store plaintext; only bcrypt hash

### Token Security

1. **Generation**: Cryptographically secure random tokens (32 bytes)
2. **Encoding**: Base64 URL-safe encoding
3. **Expiration**:
   - Email verification: 24-48 hours
   - Password reset: 1-4 hours
4. **Single Use**: Tokens marked as `used_at` after consumption

### OAuth Integration

1. **Zitadel User ID Mapping**: One-to-one relationship
2. **Token Validation**: Verify JWT signatures from Zitadel
3. **Refresh Tokens**: Stored securely in application layer (not in DB)

### SQL Injection Prevention

1. **Parameterized Queries**: Always use query parameters
2. **Check Constraints**: Validate email format at DB level
3. **Prepared Statements**: Use in application code

### Audit Trail

1. **Immutable Logs**: `auth_audit_log` has no UPDATE/DELETE policies
2. **IP Address Tracking**: Store originating IP for all auth events
3. **Failed Login Detection**: Query audit log for brute-force patterns
4. **Retention Policy**: Keep audit logs for 90 days minimum (compliance)

### GDPR Compliance

1. **Soft Deletes**: `deleted_at` timestamp for user data retention
2. **Right to Deletion**: Implement hard delete after retention period
3. **Data Export**: Query all user-related data across tables
4. **Consent Tracking**: Store in `event_data` JSONB field

---

## Integration Points

### Zitadel OAuth 2.0 Integration

**User Registration Flow:**
```
1. User registers in Family Hub
2. Create user in auth.users (password_hash, email)
3. Trigger Zitadel user creation via API
4. Store zitadel_user_id in auth.users
5. Send email verification
```

**OAuth Login Flow:**
```
1. User authenticates via Zitadel (OAuth 2.0 / OIDC)
2. Receive JWT access token from Zitadel
3. Validate token signature
4. Extract zitadel_user_id from JWT claims
5. Query auth.users WHERE zitadel_user_id = [claim]
6. Update last_login_at
7. Log event in auth_audit_log
8. Set session variable: app.current_user_id
```

### GraphQL API Integration

**User Queries:**
```graphql
type User {
  id: ID!
  email: String!
  emailVerified: Boolean!
  createdAt: DateTime!
  familyGroups: [FamilyGroup!]!
}

type Query {
  me: User!
  myFamilies: [FamilyGroup!]!
}
```

**Mutations:**
```graphql
type Mutation {
  registerUser(input: RegistrationInput!): AuthPayload!
  verifyEmail(token: String!): User!
  requestPasswordReset(email: String!): Boolean!
  resetPassword(token: String!, newPassword: String!): AuthPayload!

  createFamilyGroup(name: String!): FamilyGroup!
  inviteFamilyMember(familyId: ID!, email: String!, role: FamilyRole!): FamilyMember!
  acceptFamilyInvitation(familyId: ID!): FamilyMember!
}
```

### Email Service Integration

**Email Verification:**
- Template: `email-verification.html`
- Variables: `{user_name, verification_link}`
- Trigger: After user registration

**Password Reset:**
- Template: `password-reset.html`
- Variables: `{user_name, reset_link, expires_in_hours}`
- Trigger: On password reset request

**Family Invitation:**
- Template: `family-invitation.html`
- Variables: `{inviter_name, family_name, invitation_link}`
- Trigger: When user invited to family

### Domain Events Published

```csharp
public record UserRegisteredEvent(Guid UserId, string Email, DateTime RegisteredAt);
public record UserEmailVerifiedEvent(Guid UserId, DateTime VerifiedAt);
public record UserLoggedInEvent(Guid UserId, DateTime LoginAt, string IpAddress);
public record FamilyGroupCreatedEvent(Guid GroupId, Guid OwnerId, string Name);
public record MemberAddedToFamilyEvent(Guid GroupId, Guid UserId, FamilyRole Role);
```

**Event Bus Integration:**
- Publish via RabbitMQ (Phase 1: in-process, Phase 5+: network)
- Consumed by Communication Service for notifications
- Event sourcing for audit compliance

---

## Future Enhancements

### Phase 2+ Considerations

1. **Multi-Factor Authentication (MFA)**
   - Add table: `auth.user_mfa_settings`
   - TOTP support (authenticator apps)
   - SMS backup codes

2. **Social Login Providers**
   - Extend `users` table with provider columns
   - Add table: `auth.oauth_provider_accounts`
   - Support Google, Microsoft, Apple

3. **Session Management**
   - Add table: `auth.user_sessions`
   - Active session tracking
   - Concurrent session limits

4. **API Keys (for integrations)**
   - Add table: `auth.api_keys`
   - Scoped permissions
   - Rate limiting per key

5. **Federated Identity (Phase 7+)**
   - Cross-instance user lookups
   - ActivityPub integration
   - Instance trust relationships

---

## Maintenance Procedures

### Token Cleanup Job

Run daily to remove expired/used tokens:

```sql
-- Delete expired email verification tokens (>7 days old)
DELETE FROM auth.email_verification_tokens
WHERE expires_at < CURRENT_TIMESTAMP - INTERVAL '7 days';

-- Delete used password reset tokens (>24 hours old)
DELETE FROM auth.password_reset_tokens
WHERE used_at IS NOT NULL
AND used_at < CURRENT_TIMESTAMP - INTERVAL '24 hours';
```

### Audit Log Archival

Archive old audit logs (>90 days) to cold storage:

```sql
-- Archive to separate table
INSERT INTO auth.auth_audit_log_archive
SELECT * FROM auth.auth_audit_log
WHERE created_at < CURRENT_TIMESTAMP - INTERVAL '90 days';

DELETE FROM auth.auth_audit_log
WHERE created_at < CURRENT_TIMESTAMP - INTERVAL '90 days';
```

### GDPR Data Deletion

Hard delete users marked for deletion >30 days ago:

```sql
-- Permanently delete users and cascade
DELETE FROM auth.users
WHERE deleted_at IS NOT NULL
AND deleted_at < CURRENT_TIMESTAMP - INTERVAL '30 days';
```

---

## Appendix

### Database Size Estimates

**Initial (MVP with 100 users):**
- `users`: ~10 KB
- `families`: ~5 KB
- `user_families`: ~15 KB
- `auth_audit_log`: ~500 KB (with active usage)
- Total: ~1 MB

**At Scale (10,000 users, 2,000 families):**
- `users`: ~1 MB
- `families`: ~200 KB
- `user_families`: ~2 MB
- `auth_audit_log`: ~500 MB/year
- Total: ~3 MB + logs

### Backup Strategy

1. **Continuous Archiving**: WAL archiving to S3/Azure Blob
2. **Daily Full Backups**: pg_dump scheduled via cron
3. **Point-in-Time Recovery**: Enabled with WAL retention
4. **Backup Retention**: 30 days online, 1 year archived

### Disaster Recovery

- **RTO** (Recovery Time Objective): < 1 hour
- **RPO** (Recovery Point Objective): < 5 minutes
- **Replication**: Streaming replication to standby (Phase 2+)
- **Failover**: Automated with Patroni (Phase 3+)

---

**Document Version:** 1.0
**Last Updated:** 2025-12-22
**Author:** Database Administrator Agent (Claude Code)
**Status:** Ready for Implementation
