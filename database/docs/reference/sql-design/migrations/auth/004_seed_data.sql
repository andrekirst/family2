-- ============================================================================
-- Migration: 004_seed_data.sql
-- Description: Seed development and test data for Auth Module
-- Author: Database Administrator Agent (Claude Code)
-- Date: 2025-12-22
-- Version: 1.0
--
-- WARNING: This script is for DEVELOPMENT and TESTING ONLY
-- DO NOT run in production environments
-- ============================================================================

-- Check environment before proceeding
DO $$
BEGIN
    IF current_database() = 'family_hub_production' THEN
        RAISE EXCEPTION 'Seed data cannot be loaded in production database!';
    END IF;

    RAISE NOTICE 'Loading seed data for development/testing environment: %', current_database();
END;
$$;


-- Seed test users
-- ============================================================================
-- Note: All passwords are 'Password123!' hashed with bcrypt cost factor 12
-- In real applications, never store plaintext passwords or use weak passwords

INSERT INTO auth.users (id, email, password_hash, email_verified, email_verified_at, last_login_at)
VALUES
    -- Test user 1: Verified email, family owner
    (
        '00000000-0000-0000-0000-000000000001'::UUID,
        'john.smith@example.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYfY5z5mZ5y',  -- Password123!
        TRUE,
        CURRENT_TIMESTAMP - INTERVAL '30 days',
        CURRENT_TIMESTAMP - INTERVAL '1 day'
    ),

    -- Test user 2: Verified email, family member
    (
        '00000000-0000-0000-0000-000000000002'::UUID,
        'jane.smith@example.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYfY5z5mZ5y',
        TRUE,
        CURRENT_TIMESTAMP - INTERVAL '25 days',
        CURRENT_TIMESTAMP - INTERVAL '2 hours'
    ),

    -- Test user 3: Unverified email
    (
        '00000000-0000-0000-0000-000000000003'::UUID,
        'bob.jones@example.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYfY5z5mZ5y',
        FALSE,
        NULL,
        NULL
    ),

    -- Test user 4: Child account
    (
        '00000000-0000-0000-0000-000000000004'::UUID,
        'tommy.smith@example.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYfY5z5mZ5y',
        TRUE,
        CURRENT_TIMESTAMP - INTERVAL '20 days',
        CURRENT_TIMESTAMP - INTERVAL '3 hours'
    ),

    -- Test user 5: Admin in multiple families
    (
        '00000000-0000-0000-0000-000000000005'::UUID,
        'sarah.admin@example.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYfY5z5mZ5y',
        TRUE,
        CURRENT_TIMESTAMP - INTERVAL '60 days',
        CURRENT_TIMESTAMP - INTERVAL '30 minutes'
    ),

    -- Test user 6: Zitadel OAuth user
    (
        '00000000-0000-0000-0000-000000000006'::UUID,
        'oauth.user@example.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYfY5z5mZ5y',
        TRUE,
        CURRENT_TIMESTAMP - INTERVAL '15 days',
        CURRENT_TIMESTAMP - INTERVAL '1 hour'
    ),

    -- Test user 7: Pending invitation
    (
        '00000000-0000-0000-0000-000000000007'::UUID,
        'invited.user@example.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYfY5z5mZ5y',
        TRUE,
        CURRENT_TIMESTAMP - INTERVAL '5 days',
        NULL
    )
ON CONFLICT (id) DO NOTHING;

-- Update Zitadel user ID for OAuth test user
UPDATE auth.users
SET zitadel_user_id = 'zitadel_test_user_123456'
WHERE id = '00000000-0000-0000-0000-000000000006'::UUID;


-- Seed test families
-- ============================================================================

INSERT INTO auth.families (id, name, owner_id, created_at)
VALUES
    -- Smith Family (owner: john.smith@example.com)
    (
        '10000000-0000-0000-0000-000000000001'::UUID,
        'Smith Family',
        '00000000-0000-0000-0000-000000000001'::UUID,
        CURRENT_TIMESTAMP - INTERVAL '30 days'
    ),

    -- Extended Smith Family (owner: sarah.admin@example.com)
    (
        '10000000-0000-0000-0000-000000000002'::UUID,
        'Extended Smith Family',
        '00000000-0000-0000-0000-000000000005'::UUID,
        CURRENT_TIMESTAMP - INTERVAL '60 days'
    ),

    -- Jones Family (owner: bob.jones@example.com, unverified email)
    (
        '10000000-0000-0000-0000-000000000003'::UUID,
        'Jones Family',
        '00000000-0000-0000-0000-000000000003'::UUID,
        CURRENT_TIMESTAMP - INTERVAL '10 days'
    )
ON CONFLICT (id) DO NOTHING;


-- Seed family memberships
-- ============================================================================
-- Note: Family owners are automatically added by trigger, but we include them explicitly for clarity

INSERT INTO auth.user_families (id, user_id, family_id, role, is_active, invited_by, joined_at, invitation_accepted_at)
VALUES
    -- Smith Family memberships
    (
        '20000000-0000-0000-0000-000000000001'::UUID,
        '00000000-0000-0000-0000-000000000001'::UUID,  -- john.smith (owner)
        '10000000-0000-0000-0000-000000000001'::UUID,  -- Smith Family
        'owner',
        TRUE,
        NULL,
        CURRENT_TIMESTAMP - INTERVAL '30 days',
        CURRENT_TIMESTAMP - INTERVAL '30 days'
    ),
    (
        '20000000-0000-0000-0000-000000000002'::UUID,
        '00000000-0000-0000-0000-000000000002'::UUID,  -- jane.smith (admin)
        '10000000-0000-0000-0000-000000000001'::UUID,
        'admin',
        TRUE,
        '00000000-0000-0000-0000-000000000001'::UUID,  -- invited by john
        CURRENT_TIMESTAMP - INTERVAL '25 days',
        CURRENT_TIMESTAMP - INTERVAL '25 days'
    ),
    (
        '20000000-0000-0000-0000-000000000003'::UUID,
        '00000000-0000-0000-0000-000000000004'::UUID,  -- tommy.smith (child)
        '10000000-0000-0000-0000-000000000001'::UUID,
        'child',
        TRUE,
        '00000000-0000-0000-0000-000000000001'::UUID,
        CURRENT_TIMESTAMP - INTERVAL '20 days',
        CURRENT_TIMESTAMP - INTERVAL '20 days'
    ),

    -- Extended Smith Family memberships
    (
        '20000000-0000-0000-0000-000000000004'::UUID,
        '00000000-0000-0000-0000-000000000005'::UUID,  -- sarah.admin (owner)
        '10000000-0000-0000-0000-000000000002'::UUID,  -- Extended Smith Family
        'owner',
        TRUE,
        NULL,
        CURRENT_TIMESTAMP - INTERVAL '60 days',
        CURRENT_TIMESTAMP - INTERVAL '60 days'
    ),
    (
        '20000000-0000-0000-0000-000000000005'::UUID,
        '00000000-0000-0000-0000-000000000001'::UUID,  -- john.smith (member)
        '10000000-0000-0000-0000-000000000002'::UUID,
        'member',
        TRUE,
        '00000000-0000-0000-0000-000000000005'::UUID,
        CURRENT_TIMESTAMP - INTERVAL '55 days',
        CURRENT_TIMESTAMP - INTERVAL '55 days'
    ),
    (
        '20000000-0000-0000-0000-000000000006'::UUID,
        '00000000-0000-0000-0000-000000000006'::UUID,  -- oauth.user (member)
        '10000000-0000-0000-0000-000000000002'::UUID,
        'member',
        TRUE,
        '00000000-0000-0000-0000-000000000005'::UUID,
        CURRENT_TIMESTAMP - INTERVAL '15 days',
        CURRENT_TIMESTAMP - INTERVAL '15 days'
    ),

    -- Pending invitation (not accepted yet)
    (
        '20000000-0000-0000-0000-000000000007'::UUID,
        '00000000-0000-0000-0000-000000000007'::UUID,  -- invited.user
        '10000000-0000-0000-0000-000000000001'::UUID,  -- Smith Family
        'member',
        TRUE,
        '00000000-0000-0000-0000-000000000001'::UUID,
        CURRENT_TIMESTAMP - INTERVAL '3 days',
        NULL  -- Not accepted yet
    ),

    -- Jones Family (owner only, unverified)
    (
        '20000000-0000-0000-0000-000000000008'::UUID,
        '00000000-0000-0000-0000-000000000003'::UUID,  -- bob.jones (owner)
        '10000000-0000-0000-0000-000000000003'::UUID,  -- Jones Family
        'owner',
        TRUE,
        NULL,
        CURRENT_TIMESTAMP - INTERVAL '10 days',
        CURRENT_TIMESTAMP - INTERVAL '10 days'
    )
ON CONFLICT (user_id, family_id) DO NOTHING;


-- Seed email verification tokens
-- ============================================================================

INSERT INTO auth.email_verification_tokens (id, user_id, token, expires_at, created_at, used_at)
VALUES
    -- Active token for unverified user (bob.jones)
    (
        '30000000-0000-0000-0000-000000000001'::UUID,
        '00000000-0000-0000-0000-000000000003'::UUID,
        'verify_email_bob_jones_active_token_123',
        CURRENT_TIMESTAMP + INTERVAL '24 hours',
        CURRENT_TIMESTAMP - INTERVAL '2 hours',
        NULL
    ),

    -- Used token (john.smith)
    (
        '30000000-0000-0000-0000-000000000002'::UUID,
        '00000000-0000-0000-0000-000000000001'::UUID,
        'verify_email_john_smith_used_token_456',
        CURRENT_TIMESTAMP - INTERVAL '29 days',
        CURRENT_TIMESTAMP - INTERVAL '30 days',
        CURRENT_TIMESTAMP - INTERVAL '30 days'
    ),

    -- Expired token (invited.user)
    (
        '30000000-0000-0000-0000-000000000003'::UUID,
        '00000000-0000-0000-0000-000000000007'::UUID,
        'verify_email_expired_token_789',
        CURRENT_TIMESTAMP - INTERVAL '1 day',
        CURRENT_TIMESTAMP - INTERVAL '3 days',
        NULL
    )
ON CONFLICT (id) DO NOTHING;


-- Seed password reset tokens
-- ============================================================================

INSERT INTO auth.password_reset_tokens (id, user_id, token, expires_at, created_at, used_at)
VALUES
    -- Active reset token for jane.smith
    (
        '40000000-0000-0000-0000-000000000001'::UUID,
        '00000000-0000-0000-0000-000000000002'::UUID,
        'reset_password_jane_smith_active_abc123',
        CURRENT_TIMESTAMP + INTERVAL '1 hour',
        CURRENT_TIMESTAMP - INTERVAL '30 minutes',
        NULL
    ),

    -- Used reset token for john.smith
    (
        '40000000-0000-0000-0000-000000000002'::UUID,
        '00000000-0000-0000-0000-000000000001'::UUID,
        'reset_password_john_smith_used_def456',
        CURRENT_TIMESTAMP - INTERVAL '2 days',
        CURRENT_TIMESTAMP - INTERVAL '3 days',
        CURRENT_TIMESTAMP - INTERVAL '2 days'
    ),

    -- Expired reset token for oauth.user
    (
        '40000000-0000-0000-0000-000000000003'::UUID,
        '00000000-0000-0000-0000-000000000006'::UUID,
        'reset_password_expired_ghi789',
        CURRENT_TIMESTAMP - INTERVAL '2 hours',
        CURRENT_TIMESTAMP - INTERVAL '6 hours',
        NULL
    )
ON CONFLICT (id) DO NOTHING;


-- Seed audit log entries
-- ============================================================================
-- Note: Some audit entries are created automatically by triggers,
-- but we add additional test data for various scenarios

INSERT INTO auth.auth_audit_log (id, user_id, event_type, event_data, ip_address, user_agent, success, failure_reason, created_at)
VALUES
    -- Successful login events
    (
        '50000000-0000-0000-0000-000000000001'::UUID,
        '00000000-0000-0000-0000-000000000001'::UUID,
        'user_logged_in',
        '{"method": "password"}'::JSONB,
        '192.168.1.100'::INET,
        'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36',
        TRUE,
        NULL,
        CURRENT_TIMESTAMP - INTERVAL '1 day'
    ),
    (
        '50000000-0000-0000-0000-000000000002'::UUID,
        '00000000-0000-0000-0000-000000000002'::UUID,
        'user_logged_in',
        '{"method": "password"}'::JSONB,
        '192.168.1.101'::INET,
        'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)',
        TRUE,
        NULL,
        CURRENT_TIMESTAMP - INTERVAL '2 hours'
    ),

    -- Failed login attempts (suspicious activity)
    (
        '50000000-0000-0000-0000-000000000003'::UUID,
        NULL,  -- User unknown for failed attempts
        'login_failed',
        '{"attempted_email": "john.smith@example.com"}'::JSONB,
        '203.0.113.42'::INET,  -- Suspicious IP
        'curl/7.68.0',
        FALSE,
        'Invalid credentials',
        CURRENT_TIMESTAMP - INTERVAL '3 hours'
    ),
    (
        '50000000-0000-0000-0000-000000000004'::UUID,
        NULL,
        'login_failed',
        '{"attempted_email": "admin@example.com"}'::JSONB,
        '203.0.113.42'::INET,  -- Same suspicious IP
        'curl/7.68.0',
        FALSE,
        'Invalid credentials',
        CURRENT_TIMESTAMP - INTERVAL '2 hours 55 minutes'
    ),

    -- Password reset events
    (
        '50000000-0000-0000-0000-000000000005'::UUID,
        '00000000-0000-0000-0000-000000000002'::UUID,
        'password_reset_requested',
        '{}'::JSONB,
        '192.168.1.101'::INET,
        'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)',
        TRUE,
        NULL,
        CURRENT_TIMESTAMP - INTERVAL '30 minutes'
    ),

    -- OAuth login
    (
        '50000000-0000-0000-0000-000000000006'::UUID,
        '00000000-0000-0000-0000-000000000006'::UUID,
        'user_logged_in',
        '{"method": "oauth", "provider": "zitadel"}'::JSONB,
        '192.168.1.102'::INET,
        'Mozilla/5.0 (X11; Linux x86_64)',
        TRUE,
        NULL,
        CURRENT_TIMESTAMP - INTERVAL '1 hour'
    ),

    -- Family invitation sent
    (
        '50000000-0000-0000-0000-000000000007'::UUID,
        '00000000-0000-0000-0000-000000000001'::UUID,
        'family_member_invited',
        '{"family_id": "10000000-0000-0000-0000-000000000001", "invited_user": "invited.user@example.com"}'::JSONB,
        '192.168.1.100'::INET,
        'Mozilla/5.0 (Windows NT 10.0; Win64; x64)',
        TRUE,
        NULL,
        CURRENT_TIMESTAMP - INTERVAL '3 days'
    )
ON CONFLICT (id) DO NOTHING;


-- Verify seed data
-- ============================================================================

DO $$
DECLARE
    user_count INTEGER;
    family_count INTEGER;
    membership_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO user_count FROM auth.users WHERE deleted_at IS NULL;
    SELECT COUNT(*) INTO family_count FROM auth.families WHERE deleted_at IS NULL;
    SELECT COUNT(*) INTO membership_count FROM auth.user_families WHERE is_active = TRUE;

    RAISE NOTICE '=================================================================';
    RAISE NOTICE 'Seed data loaded successfully!';
    RAISE NOTICE '=================================================================';
    RAISE NOTICE 'Users created: %', user_count;
    RAISE NOTICE 'Families created: %', family_count;
    RAISE NOTICE 'Active memberships: %', membership_count;
    RAISE NOTICE '';
    RAISE NOTICE 'Test Credentials (all passwords: Password123!):';
    RAISE NOTICE '  - john.smith@example.com (Owner, Smith Family)';
    RAISE NOTICE '  - jane.smith@example.com (Admin, Smith Family)';
    RAISE NOTICE '  - bob.jones@example.com (Unverified email)';
    RAISE NOTICE '  - tommy.smith@example.com (Child account)';
    RAISE NOTICE '  - sarah.admin@example.com (Multi-family admin)';
    RAISE NOTICE '  - oauth.user@example.com (OAuth/Zitadel user)';
    RAISE NOTICE '  - invited.user@example.com (Pending invitation)';
    RAISE NOTICE '=================================================================';
END;
$$;


-- ============================================================================
-- Seed data completed successfully
-- ============================================================================
