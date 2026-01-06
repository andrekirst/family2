-- ============================================================================
-- Rollback Script: Phase 0.2 Data Migration (CHILD → MANAGED_ACCOUNT)
-- ============================================================================
-- Epic: #24 - Family Member Invitation System
-- Migration: 20260104215404_MigrateChildToManagedAccount
-- Purpose: Rollback data migration (if needed)
-- Date: 2026-01-04
-- ============================================================================

-- NOTE: This migration was verification-only. No actual data was changed.
-- Therefore, this rollback script is also a NO-OP.

BEGIN;

-- Verification: Check if role column exists (it shouldn't)
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'auth'
          AND table_name = 'users'
          AND column_name = 'role'
    ) THEN
        RAISE EXCEPTION 'Unexpected: role column exists in auth.users table';
    ELSE
        RAISE NOTICE 'Verification: No role column in auth.users table';
    END IF;
END $$;

-- Log rollback attempt for audit trail
DO $$
BEGIN
    RAISE NOTICE '=======================================================';
    RAISE NOTICE 'Rollback Phase 0.2: MigrateChildToManagedAccount';
    RAISE NOTICE '=======================================================';
    RAISE NOTICE 'Status: NO-OP (original migration was verification-only)';
    RAISE NOTICE 'UserRole is NOT persisted in database - roles are computed dynamically';
    RAISE NOTICE 'No data rollback required';
    RAISE NOTICE '=======================================================';
END $$;

-- NO actual rollback operations needed

COMMIT;

-- ============================================================================
-- Post-Rollback Verification
-- ============================================================================

-- Check current user count
SELECT
    COUNT(*) as total_users,
    COUNT(*) FILTER (WHERE deleted_at IS NULL) as active_users,
    COUNT(*) FILTER (WHERE deleted_at IS NOT NULL) as deleted_users
FROM auth.users;

-- Verify migration history
SELECT migration_id, product_version
FROM "__EFMigrationsHistory"
ORDER BY migration_id DESC
LIMIT 5;

-- ============================================================================
-- Expected Outcome
-- ============================================================================
-- After running this script:
-- 1. No data changes (UserRole not stored in database)
-- 2. Migration can be removed via: dotnet ef migrations remove
-- 3. Application continues functioning normally
-- ============================================================================
