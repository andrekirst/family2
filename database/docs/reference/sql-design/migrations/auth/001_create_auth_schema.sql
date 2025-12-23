-- ============================================================================
-- Migration: 001_create_auth_schema.sql
-- Description: Create auth schema and core tables for user authentication
-- Author: Database Administrator Agent (Claude Code)
-- Date: 2025-12-22
-- Version: 1.0
-- ============================================================================

-- Create auth schema
-- ============================================================================
CREATE SCHEMA IF NOT EXISTS auth;

COMMENT ON SCHEMA auth IS 'Authentication and authorization module';


-- Create users table
-- ============================================================================
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
    CONSTRAINT users_password_hash_check CHECK (length(password_hash) >= 60),
    CONSTRAINT users_email_verified_consistency CHECK (
        (email_verified = TRUE AND email_verified_at IS NOT NULL) OR
        (email_verified = FALSE)
    )
);

-- Indexes for users table
CREATE INDEX idx_users_email ON auth.users(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_zitadel_user_id ON auth.users(zitadel_user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_created_at ON auth.users(created_at);
CREATE INDEX idx_users_deleted_at ON auth.users(deleted_at) WHERE deleted_at IS NOT NULL;

-- Comments for users table
COMMENT ON TABLE auth.users IS 'Core user identity and authentication';
COMMENT ON COLUMN auth.users.id IS 'Primary key (UUID v4)';
COMMENT ON COLUMN auth.users.email IS 'User email address (unique, case-insensitive)';
COMMENT ON COLUMN auth.users.password_hash IS 'bcrypt hashed password (60 characters minimum)';
COMMENT ON COLUMN auth.users.email_verified IS 'Whether email has been verified';
COMMENT ON COLUMN auth.users.email_verified_at IS 'Timestamp of email verification';
COMMENT ON COLUMN auth.users.zitadel_user_id IS 'External Zitadel OAuth 2.0 user identifier';
COMMENT ON COLUMN auth.users.created_at IS 'User registration timestamp';
COMMENT ON COLUMN auth.users.updated_at IS 'Last update timestamp';
COMMENT ON COLUMN auth.users.deleted_at IS 'Soft delete timestamp for GDPR compliance';
COMMENT ON COLUMN auth.users.last_login_at IS 'Last successful login timestamp';


-- Create families table
-- ============================================================================
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

-- Indexes for families table
CREATE INDEX idx_families_owner_id ON auth.families(owner_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_families_created_at ON auth.families(created_at);
CREATE INDEX idx_families_deleted_at ON auth.families(deleted_at) WHERE deleted_at IS NOT NULL;

-- Comments for families table
COMMENT ON TABLE auth.families IS 'Family groups for organizing users';
COMMENT ON COLUMN auth.families.id IS 'Primary key (UUID v4)';
COMMENT ON COLUMN auth.families.name IS 'Family group display name';
COMMENT ON COLUMN auth.families.owner_id IS 'User who created and owns the family group';
COMMENT ON COLUMN auth.families.created_at IS 'Family creation timestamp';
COMMENT ON COLUMN auth.families.updated_at IS 'Last update timestamp';
COMMENT ON COLUMN auth.families.deleted_at IS 'Soft delete timestamp';


-- Create user_families junction table
-- ============================================================================
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

-- Indexes for user_families table
CREATE INDEX idx_user_families_user_id ON auth.user_families(user_id) WHERE is_active = TRUE;
CREATE INDEX idx_user_families_family_id ON auth.user_families(family_id) WHERE is_active = TRUE;
CREATE INDEX idx_user_families_role ON auth.user_families(family_id, role);
CREATE INDEX idx_user_families_invited_by ON auth.user_families(invited_by);

-- Comments for user_families table
COMMENT ON TABLE auth.user_families IS 'Many-to-many relationship between users and families with roles';
COMMENT ON COLUMN auth.user_families.id IS 'Primary key (UUID v4)';
COMMENT ON COLUMN auth.user_families.user_id IS 'Reference to user';
COMMENT ON COLUMN auth.user_families.family_id IS 'Reference to family';
COMMENT ON COLUMN auth.user_families.role IS 'Family role: owner, admin, member, child';
COMMENT ON COLUMN auth.user_families.is_active IS 'Whether membership is currently active';
COMMENT ON COLUMN auth.user_families.invited_by IS 'User who sent the family invitation';
COMMENT ON COLUMN auth.user_families.joined_at IS 'Timestamp when user joined family';
COMMENT ON COLUMN auth.user_families.invitation_accepted_at IS 'Timestamp when invitation was accepted';


-- Create email_verification_tokens table
-- ============================================================================
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

-- Indexes for email_verification_tokens table
CREATE INDEX idx_email_verification_tokens_token ON auth.email_verification_tokens(token)
    WHERE used_at IS NULL AND expires_at > CURRENT_TIMESTAMP;
CREATE INDEX idx_email_verification_tokens_user_id ON auth.email_verification_tokens(user_id);
CREATE INDEX idx_email_verification_tokens_expires_at ON auth.email_verification_tokens(expires_at);

-- Comments for email_verification_tokens table
COMMENT ON TABLE auth.email_verification_tokens IS 'Tokens for email verification workflow';
COMMENT ON COLUMN auth.email_verification_tokens.id IS 'Primary key (UUID v4)';
COMMENT ON COLUMN auth.email_verification_tokens.user_id IS 'Reference to user';
COMMENT ON COLUMN auth.email_verification_tokens.token IS 'Cryptographically secure random token (URL-safe, base64 encoded)';
COMMENT ON COLUMN auth.email_verification_tokens.expires_at IS 'Token expiration timestamp (typically 24-48 hours from creation)';
COMMENT ON COLUMN auth.email_verification_tokens.created_at IS 'Token creation timestamp';
COMMENT ON COLUMN auth.email_verification_tokens.used_at IS 'Timestamp when token was used (NULL if unused)';


-- Create password_reset_tokens table
-- ============================================================================
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

-- Indexes for password_reset_tokens table
CREATE INDEX idx_password_reset_tokens_token ON auth.password_reset_tokens(token)
    WHERE used_at IS NULL AND expires_at > CURRENT_TIMESTAMP;
CREATE INDEX idx_password_reset_tokens_user_id ON auth.password_reset_tokens(user_id);
CREATE INDEX idx_password_reset_tokens_expires_at ON auth.password_reset_tokens(expires_at);

-- Comments for password_reset_tokens table
COMMENT ON TABLE auth.password_reset_tokens IS 'Tokens for password reset workflow';
COMMENT ON COLUMN auth.password_reset_tokens.id IS 'Primary key (UUID v4)';
COMMENT ON COLUMN auth.password_reset_tokens.user_id IS 'Reference to user';
COMMENT ON COLUMN auth.password_reset_tokens.token IS 'Cryptographically secure random token (URL-safe, base64 encoded)';
COMMENT ON COLUMN auth.password_reset_tokens.expires_at IS 'Token expiration timestamp (typically 1-4 hours from creation)';
COMMENT ON COLUMN auth.password_reset_tokens.created_at IS 'Token creation timestamp';
COMMENT ON COLUMN auth.password_reset_tokens.used_at IS 'Timestamp when token was used (NULL if unused)';


-- Create auth_audit_log table
-- ============================================================================
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

-- Indexes for auth_audit_log table
CREATE INDEX idx_auth_audit_log_user_id ON auth.auth_audit_log(user_id, created_at DESC);
CREATE INDEX idx_auth_audit_log_event_type ON auth.auth_audit_log(event_type, created_at DESC);
CREATE INDEX idx_auth_audit_log_created_at ON auth.auth_audit_log(created_at DESC);
CREATE INDEX idx_auth_audit_log_ip_address ON auth.auth_audit_log(ip_address, created_at DESC);
CREATE INDEX idx_auth_audit_log_success ON auth.auth_audit_log(success, created_at DESC) WHERE success = FALSE;
CREATE INDEX idx_auth_audit_log_event_data ON auth.auth_audit_log USING gin(event_data);

-- Comments for auth_audit_log table
COMMENT ON TABLE auth.auth_audit_log IS 'Audit trail for all authentication and authorization events';
COMMENT ON COLUMN auth.auth_audit_log.id IS 'Primary key (UUID v4)';
COMMENT ON COLUMN auth.auth_audit_log.user_id IS 'User associated with event (nullable for failed login attempts where user is unknown)';
COMMENT ON COLUMN auth.auth_audit_log.event_type IS 'Type of authentication event (e.g., user_registered, user_logged_in, password_changed)';
COMMENT ON COLUMN auth.auth_audit_log.event_data IS 'Additional event-specific data in JSON format';
COMMENT ON COLUMN auth.auth_audit_log.ip_address IS 'IP address of the request';
COMMENT ON COLUMN auth.auth_audit_log.user_agent IS 'User agent string from the request';
COMMENT ON COLUMN auth.auth_audit_log.success IS 'Whether the event was successful';
COMMENT ON COLUMN auth.auth_audit_log.failure_reason IS 'Reason for failure (required if success = FALSE)';
COMMENT ON COLUMN auth.auth_audit_log.created_at IS 'Event timestamp';


-- ============================================================================
-- Migration completed successfully
-- ============================================================================
