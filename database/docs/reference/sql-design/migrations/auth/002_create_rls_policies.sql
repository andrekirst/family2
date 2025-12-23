-- ============================================================================
-- Migration: 002_create_rls_policies.sql
-- Description: Enable Row-Level Security (RLS) policies for multi-tenancy
-- Author: Database Administrator Agent (Claude Code)
-- Date: 2025-12-22
-- Version: 1.0
-- ============================================================================

-- Enable Row-Level Security on all tables
-- ============================================================================
ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.families ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.user_families ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.email_verification_tokens ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.password_reset_tokens ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth.auth_audit_log ENABLE ROW LEVEL SECURITY;


-- Create helper function to get current user ID from session
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.current_user_id()
RETURNS UUID AS $$
BEGIN
    -- Get user ID from session variable set by application
    RETURN current_setting('app.current_user_id', true)::UUID;
EXCEPTION
    WHEN OTHERS THEN
        -- Return NULL if session variable not set or invalid
        RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE SECURITY DEFINER;

COMMENT ON FUNCTION auth.current_user_id() IS 'Returns UUID of currently authenticated user from session variable app.current_user_id';


-- RLS Policies for users table
-- ============================================================================

-- Users can SELECT their own record
CREATE POLICY users_select_own ON auth.users
    FOR SELECT
    USING (id = auth.current_user_id());

COMMENT ON POLICY users_select_own ON auth.users IS 'Users can only view their own user record';

-- Users can UPDATE their own record
CREATE POLICY users_update_own ON auth.users
    FOR UPDATE
    USING (id = auth.current_user_id());

COMMENT ON POLICY users_update_own ON auth.users IS 'Users can only update their own user record';

-- INSERT is handled by application service (no RLS policy needed)
-- DELETE is handled by soft delete (UPDATE policy applies)


-- RLS Policies for families table
-- ============================================================================

-- Users can SELECT families they are members of
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

COMMENT ON POLICY families_select_member ON auth.families IS 'Users can view families they are active members of';

-- Only family owner can UPDATE family
CREATE POLICY families_update_owner ON auth.families
    FOR UPDATE
    USING (owner_id = auth.current_user_id());

COMMENT ON POLICY families_update_owner ON auth.families IS 'Only family owner can update family details';

-- Only family owner can DELETE family (soft delete)
CREATE POLICY families_delete_owner ON auth.families
    FOR DELETE
    USING (owner_id = auth.current_user_id());

COMMENT ON POLICY families_delete_owner ON auth.families IS 'Only family owner can delete family';

-- INSERT is handled by application service (no RLS policy needed)


-- RLS Policies for user_families table
-- ============================================================================

-- Users can SELECT memberships of families they belong to
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

COMMENT ON POLICY user_families_select_member ON auth.user_families IS 'Users can view all memberships of families they belong to';

-- Users can UPDATE only their own membership record
CREATE POLICY user_families_update_own ON auth.user_families
    FOR UPDATE
    USING (user_id = auth.current_user_id());

COMMENT ON POLICY user_families_update_own ON auth.user_families IS 'Users can only update their own family membership';

-- Family owners/admins can UPDATE any membership in their family
CREATE POLICY user_families_update_admin ON auth.user_families
    FOR UPDATE
    USING (
        family_id IN (
            SELECT uf.family_id
            FROM auth.user_families uf
            WHERE uf.user_id = auth.current_user_id()
            AND uf.is_active = TRUE
            AND uf.role IN ('owner', 'admin')
        )
    );

COMMENT ON POLICY user_families_update_admin ON auth.user_families IS 'Family owners and admins can update memberships in their family';

-- Family owners/admins can DELETE memberships in their family
CREATE POLICY user_families_delete_admin ON auth.user_families
    FOR DELETE
    USING (
        family_id IN (
            SELECT uf.family_id
            FROM auth.user_families uf
            WHERE uf.user_id = auth.current_user_id()
            AND uf.is_active = TRUE
            AND uf.role IN ('owner', 'admin')
        )
    );

COMMENT ON POLICY user_families_delete_admin ON auth.user_families IS 'Family owners and admins can remove members from their family';


-- RLS Policies for email_verification_tokens table
-- ============================================================================

-- Users can SELECT only their own verification tokens
CREATE POLICY email_verification_tokens_select_own ON auth.email_verification_tokens
    FOR SELECT
    USING (user_id = auth.current_user_id());

COMMENT ON POLICY email_verification_tokens_select_own ON auth.email_verification_tokens IS 'Users can only view their own email verification tokens';

-- Users can UPDATE only their own verification tokens (to mark as used)
CREATE POLICY email_verification_tokens_update_own ON auth.email_verification_tokens
    FOR UPDATE
    USING (user_id = auth.current_user_id());

COMMENT ON POLICY email_verification_tokens_update_own ON auth.email_verification_tokens IS 'Users can update their own verification tokens';


-- RLS Policies for password_reset_tokens table
-- ============================================================================

-- Users can SELECT only their own password reset tokens
CREATE POLICY password_reset_tokens_select_own ON auth.password_reset_tokens
    FOR SELECT
    USING (user_id = auth.current_user_id());

COMMENT ON POLICY password_reset_tokens_select_own ON auth.password_reset_tokens IS 'Users can only view their own password reset tokens';

-- Users can UPDATE only their own password reset tokens (to mark as used)
CREATE POLICY password_reset_tokens_update_own ON auth.password_reset_tokens
    FOR UPDATE
    USING (user_id = auth.current_user_id());

COMMENT ON POLICY password_reset_tokens_update_own ON auth.password_reset_tokens IS 'Users can update their own password reset tokens';


-- RLS Policies for auth_audit_log table
-- ============================================================================

-- Users can SELECT only their own audit log entries
CREATE POLICY auth_audit_log_select_own ON auth.auth_audit_log
    FOR SELECT
    USING (user_id = auth.current_user_id());

COMMENT ON POLICY auth_audit_log_select_own ON auth.auth_audit_log IS 'Users can only view their own audit log entries';

-- Audit log is append-only (no UPDATE or DELETE policies)


-- Create service role for backend application (bypasses RLS)
-- ============================================================================
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'family_hub_service') THEN
        CREATE ROLE family_hub_service;
        COMMENT ON ROLE family_hub_service IS 'Service account for Family Hub backend (bypasses RLS)';
    END IF;
END
$$;

-- Grant service role bypass RLS privilege
ALTER ROLE family_hub_service BYPASSRLS;

-- Grant service role access to auth schema
GRANT USAGE ON SCHEMA auth TO family_hub_service;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA auth TO family_hub_service;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA auth TO family_hub_service;

-- Ensure future tables are also granted to service role
ALTER DEFAULT PRIVILEGES IN SCHEMA auth GRANT ALL ON TABLES TO family_hub_service;
ALTER DEFAULT PRIVILEGES IN SCHEMA auth GRANT ALL ON SEQUENCES TO family_hub_service;


-- ============================================================================
-- RLS Setup completed successfully
-- ============================================================================
