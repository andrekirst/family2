-- ============================================================================
-- Migration: 003_create_triggers.sql
-- Description: Create database triggers for automated workflows
-- Author: Database Administrator Agent (Claude Code)
-- Date: 2025-12-22
-- Version: 1.0
-- ============================================================================

-- Trigger function: Update updated_at timestamp
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION auth.update_updated_at_column() IS 'Automatically updates updated_at column on row modification';


-- Apply updated_at trigger to users table
-- ============================================================================
CREATE TRIGGER users_updated_at
    BEFORE UPDATE ON auth.users
    FOR EACH ROW
    EXECUTE FUNCTION auth.update_updated_at_column();

COMMENT ON TRIGGER users_updated_at ON auth.users IS 'Automatically updates updated_at timestamp on user modification';


-- Apply updated_at trigger to families table
-- ============================================================================
CREATE TRIGGER families_updated_at
    BEFORE UPDATE ON auth.families
    FOR EACH ROW
    EXECUTE FUNCTION auth.update_updated_at_column();

COMMENT ON TRIGGER families_updated_at ON auth.families IS 'Automatically updates updated_at timestamp on family modification';


-- Trigger function: Validate family owner is active member
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.validate_family_owner_membership()
RETURNS TRIGGER AS $$
BEGIN
    -- Ensure family owner is automatically added as active member with owner role
    INSERT INTO auth.user_families (user_id, family_id, role, is_active, invitation_accepted_at)
    VALUES (NEW.owner_id, NEW.id, 'owner', TRUE, CURRENT_TIMESTAMP)
    ON CONFLICT (user_id, family_id) DO NOTHING;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION auth.validate_family_owner_membership() IS 'Automatically adds family owner as active member when family is created';


-- Apply family owner membership trigger
-- ============================================================================
CREATE TRIGGER families_ensure_owner_membership
    AFTER INSERT ON auth.families
    FOR EACH ROW
    EXECUTE FUNCTION auth.validate_family_owner_membership();

COMMENT ON TRIGGER families_ensure_owner_membership ON auth.families IS 'Ensures family owner is automatically added as active member';


-- Trigger function: Prevent owner role changes
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.protect_owner_role()
RETURNS TRIGGER AS $$
BEGIN
    -- Prevent changing owner role to non-owner (ownership transfer must be explicit)
    IF OLD.role = 'owner' AND NEW.role != 'owner' THEN
        -- Check if this is the family owner
        IF EXISTS (
            SELECT 1 FROM auth.families
            WHERE id = NEW.family_id
            AND owner_id = OLD.user_id
        ) THEN
            RAISE EXCEPTION 'Cannot change owner role. Use ownership transfer operation instead.';
        END IF;
    END IF;

    -- Prevent assigning owner role to non-owner users
    IF OLD.role != 'owner' AND NEW.role = 'owner' THEN
        IF NOT EXISTS (
            SELECT 1 FROM auth.families
            WHERE id = NEW.family_id
            AND owner_id = NEW.user_id
        ) THEN
            RAISE EXCEPTION 'Cannot assign owner role. User must be the family owner.';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION auth.protect_owner_role() IS 'Prevents unauthorized changes to owner role';


-- Apply owner role protection trigger
-- ============================================================================
CREATE TRIGGER user_families_protect_owner_role
    BEFORE UPDATE ON auth.user_families
    FOR EACH ROW
    WHEN (OLD.role IS DISTINCT FROM NEW.role)
    EXECUTE FUNCTION auth.protect_owner_role();

COMMENT ON TRIGGER user_families_protect_owner_role ON auth.user_families IS 'Protects owner role from unauthorized changes';


-- Trigger function: Prevent family owner deletion
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.prevent_owner_deletion()
RETURNS TRIGGER AS $$
BEGIN
    -- Prevent deletion of family owner membership
    IF OLD.role = 'owner' THEN
        IF EXISTS (
            SELECT 1 FROM auth.families
            WHERE id = OLD.family_id
            AND owner_id = OLD.user_id
            AND deleted_at IS NULL
        ) THEN
            RAISE EXCEPTION 'Cannot remove family owner from family. Transfer ownership first.';
        END IF;
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION auth.prevent_owner_deletion() IS 'Prevents deletion of family owner membership';


-- Apply owner deletion prevention trigger
-- ============================================================================
CREATE TRIGGER user_families_prevent_owner_deletion
    BEFORE DELETE ON auth.user_families
    FOR EACH ROW
    EXECUTE FUNCTION auth.prevent_owner_deletion();

COMMENT ON TRIGGER user_families_prevent_owner_deletion ON auth.user_families IS 'Prevents removal of family owner from family';


-- Trigger function: Auto-expire tokens
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.cleanup_expired_tokens()
RETURNS TRIGGER AS $$
BEGIN
    -- Delete expired email verification tokens (older than 7 days)
    DELETE FROM auth.email_verification_tokens
    WHERE expires_at < CURRENT_TIMESTAMP - INTERVAL '7 days';

    -- Delete used password reset tokens (older than 24 hours)
    DELETE FROM auth.password_reset_tokens
    WHERE used_at IS NOT NULL
    AND used_at < CURRENT_TIMESTAMP - INTERVAL '24 hours';

    -- Delete expired password reset tokens (older than 7 days)
    DELETE FROM auth.password_reset_tokens
    WHERE expires_at < CURRENT_TIMESTAMP - INTERVAL '7 days';

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION auth.cleanup_expired_tokens() IS 'Automatically cleans up expired and used tokens';


-- Create periodic token cleanup job (PostgreSQL 14+ with pg_cron extension)
-- Note: This requires pg_cron extension to be installed
-- If pg_cron is not available, run cleanup via application scheduled job
-- ============================================================================

-- Check if pg_cron extension is available and create scheduled job
DO $$
BEGIN
    -- Only create cron job if extension exists
    IF EXISTS (SELECT 1 FROM pg_available_extensions WHERE name = 'pg_cron') THEN
        -- Enable pg_cron extension if not already enabled
        CREATE EXTENSION IF NOT EXISTS pg_cron;

        -- Schedule daily token cleanup at 3 AM
        PERFORM cron.schedule(
            'auth-token-cleanup',
            '0 3 * * *',  -- Daily at 3 AM
            $$DELETE FROM auth.email_verification_tokens WHERE expires_at < CURRENT_TIMESTAMP - INTERVAL '7 days';
              DELETE FROM auth.password_reset_tokens WHERE used_at IS NOT NULL AND used_at < CURRENT_TIMESTAMP - INTERVAL '24 hours';
              DELETE FROM auth.password_reset_tokens WHERE expires_at < CURRENT_TIMESTAMP - INTERVAL '7 days';$$
        );

        RAISE NOTICE 'pg_cron: Token cleanup job scheduled successfully';
    ELSE
        RAISE NOTICE 'pg_cron extension not available. Token cleanup must be handled by application scheduled job.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Could not create pg_cron job: %. Token cleanup must be handled by application.', SQLERRM;
END;
$$;


-- Trigger function: Log audit events for authentication actions
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.log_user_changes()
RETURNS TRIGGER AS $$
BEGIN
    -- Log user registration
    IF TG_OP = 'INSERT' THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.id,
            'user_registered',
            jsonb_build_object(
                'email', NEW.email,
                'email_verified', NEW.email_verified
            ),
            TRUE
        );
        RETURN NEW;
    END IF;

    -- Log email verification
    IF TG_OP = 'UPDATE' AND OLD.email_verified = FALSE AND NEW.email_verified = TRUE THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.id,
            'email_verified',
            jsonb_build_object('email', NEW.email, 'verified_at', NEW.email_verified_at),
            TRUE
        );
    END IF;

    -- Log password changes
    IF TG_OP = 'UPDATE' AND OLD.password_hash != NEW.password_hash THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.id,
            'password_changed',
            jsonb_build_object('changed_at', CURRENT_TIMESTAMP),
            TRUE
        );
    END IF;

    -- Log user soft deletion
    IF TG_OP = 'UPDATE' AND OLD.deleted_at IS NULL AND NEW.deleted_at IS NOT NULL THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.id,
            'user_deleted',
            jsonb_build_object('deleted_at', NEW.deleted_at),
            TRUE
        );
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION auth.log_user_changes() IS 'Automatically logs authentication events to audit log';


-- Apply user audit logging trigger
-- ============================================================================
CREATE TRIGGER users_log_changes
    AFTER INSERT OR UPDATE ON auth.users
    FOR EACH ROW
    EXECUTE FUNCTION auth.log_user_changes();

COMMENT ON TRIGGER users_log_changes ON auth.users IS 'Logs user registration, email verification, password changes, and deletions';


-- Trigger function: Log family events
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.log_family_changes()
RETURNS TRIGGER AS $$
BEGIN
    -- Log family creation
    IF TG_OP = 'INSERT' THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.owner_id,
            'family_created',
            jsonb_build_object(
                'family_id', NEW.id,
                'family_name', NEW.name
            ),
            TRUE
        );
        RETURN NEW;
    END IF;

    -- Log family name changes
    IF TG_OP = 'UPDATE' AND OLD.name != NEW.name THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.owner_id,
            'family_updated',
            jsonb_build_object(
                'family_id', NEW.id,
                'old_name', OLD.name,
                'new_name', NEW.name
            ),
            TRUE
        );
    END IF;

    -- Log ownership transfer
    IF TG_OP = 'UPDATE' AND OLD.owner_id != NEW.owner_id THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.owner_id,
            'family_ownership_transferred',
            jsonb_build_object(
                'family_id', NEW.id,
                'family_name', NEW.name,
                'old_owner_id', OLD.owner_id,
                'new_owner_id', NEW.owner_id
            ),
            TRUE
        );
    END IF;

    -- Log family deletion
    IF TG_OP = 'UPDATE' AND OLD.deleted_at IS NULL AND NEW.deleted_at IS NOT NULL THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.owner_id,
            'family_deleted',
            jsonb_build_object(
                'family_id', NEW.id,
                'family_name', NEW.name,
                'deleted_at', NEW.deleted_at
            ),
            TRUE
        );
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION auth.log_family_changes() IS 'Automatically logs family-related events to audit log';


-- Apply family audit logging trigger
-- ============================================================================
CREATE TRIGGER families_log_changes
    AFTER INSERT OR UPDATE ON auth.families
    FOR EACH ROW
    EXECUTE FUNCTION auth.log_family_changes();

COMMENT ON TRIGGER families_log_changes ON auth.families IS 'Logs family creation, updates, ownership transfers, and deletions';


-- Trigger function: Log family membership events
-- ============================================================================
CREATE OR REPLACE FUNCTION auth.log_membership_changes()
RETURNS TRIGGER AS $$
DECLARE
    inviter_email TEXT;
BEGIN
    -- Log member invitation
    IF TG_OP = 'INSERT' THEN
        SELECT email INTO inviter_email FROM auth.users WHERE id = NEW.invited_by;

        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.user_id,
            'family_member_invited',
            jsonb_build_object(
                'family_id', NEW.family_id,
                'role', NEW.role,
                'invited_by', NEW.invited_by,
                'inviter_email', inviter_email
            ),
            TRUE
        );
        RETURN NEW;
    END IF;

    -- Log invitation acceptance
    IF TG_OP = 'UPDATE' AND OLD.invitation_accepted_at IS NULL AND NEW.invitation_accepted_at IS NOT NULL THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.user_id,
            'family_invitation_accepted',
            jsonb_build_object(
                'family_id', NEW.family_id,
                'role', NEW.role,
                'accepted_at', NEW.invitation_accepted_at
            ),
            TRUE
        );
    END IF;

    -- Log role changes
    IF TG_OP = 'UPDATE' AND OLD.role != NEW.role THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.user_id,
            'family_member_role_changed',
            jsonb_build_object(
                'family_id', NEW.family_id,
                'old_role', OLD.role,
                'new_role', NEW.role
            ),
            TRUE
        );
    END IF;

    -- Log member deactivation
    IF TG_OP = 'UPDATE' AND OLD.is_active = TRUE AND NEW.is_active = FALSE THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            NEW.user_id,
            'family_member_deactivated',
            jsonb_build_object(
                'family_id', NEW.family_id,
                'role', NEW.role
            ),
            TRUE
        );
    END IF;

    -- Log member removal
    IF TG_OP = 'DELETE' THEN
        INSERT INTO auth.auth_audit_log (user_id, event_type, event_data, success)
        VALUES (
            OLD.user_id,
            'family_member_removed',
            jsonb_build_object(
                'family_id', OLD.family_id,
                'role', OLD.role
            ),
            TRUE
        );
        RETURN OLD;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION auth.log_membership_changes() IS 'Automatically logs family membership events to audit log';


-- Apply membership audit logging trigger
-- ============================================================================
CREATE TRIGGER user_families_log_changes
    AFTER INSERT OR UPDATE OR DELETE ON auth.user_families
    FOR EACH ROW
    EXECUTE FUNCTION auth.log_membership_changes();

COMMENT ON TRIGGER user_families_log_changes ON auth.user_families IS 'Logs family member invitations, acceptances, role changes, and removals';


-- ============================================================================
-- Triggers setup completed successfully
-- ============================================================================
