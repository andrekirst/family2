-- Family Hub - Row-Level Security (RLS) Policies
-- Implements multi-tenant data isolation at the database level
--
-- SECURITY CONTEXT:
-- - PostgreSQL RLS enforces data isolation even if application logic is compromised
-- - current_user_id() function returns the authenticated user's ID (set by middleware)
-- - Policies ensure users can only access data from families they belong to
--
-- ARCHITECTURE DECISION:
-- - RLS is enabled on all family-scoped tables (families, users, family_member_invitations)
-- - outbox_events table does NOT have RLS (internal event processing, not user-facing)
-- - Policies use indexed columns (family_id, user_id) for optimal performance
--
-- PHASE 0 CRITICAL SECURITY: Without RLS, users can query other families' data via GraphQL
--
-- Last Updated: 2026-01-09

-- ========================================
-- ENABLE RLS ON TABLES
-- ========================================

-- Enable RLS on families table
-- Users should only see families they belong to
ALTER TABLE auth.families ENABLE ROW LEVEL SECURITY;

-- Enable RLS on users table
-- Users should only see other users in their family
ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

-- Enable RLS on family_member_invitations table
-- Users should only see invitations for their families
ALTER TABLE auth.family_member_invitations ENABLE ROW LEVEL SECURITY;

-- NOTE: outbox_events table does NOT have RLS enabled
-- Reason: Internal event processing table, not exposed to users via GraphQL


-- ========================================
-- RLS POLICIES FOR auth.families
-- ========================================

-- Policy: Users can read families they belong to
-- Logic: Check if current_user_id() exists in auth.users with matching family_id
CREATE POLICY family_select_policy ON auth.families
    FOR SELECT
    USING (
        id IN (
            SELECT family_id
            FROM auth.users
            WHERE id = current_user_id()
              AND deleted_at IS NULL
        )
    );

-- Policy: Only family owners can update their families
-- Logic: Check if current_user_id() is the owner_id
CREATE POLICY family_update_policy ON auth.families
    FOR UPDATE
    USING (
        owner_id = current_user_id()
        AND deleted_at IS NULL
    );

-- Policy: Only family owners can delete their families (soft delete)
-- Logic: Check if current_user_id() is the owner_id
CREATE POLICY family_delete_policy ON auth.families
    FOR DELETE
    USING (
        owner_id = current_user_id()
    );

-- Policy: Authenticated users can create families
-- Logic: current_user_id() must be set (not NULL)
CREATE POLICY family_insert_policy ON auth.families
    FOR INSERT
    WITH CHECK (
        current_user_id() IS NOT NULL
    );


-- ========================================
-- RLS POLICIES FOR auth.users
-- ========================================

-- Policy: Users can read other users in their family
-- Logic: Both the current user and the target user must belong to the same family
CREATE POLICY users_select_policy ON auth.users
    FOR SELECT
    USING (
        family_id IN (
            SELECT family_id
            FROM auth.users
            WHERE id = current_user_id()
              AND deleted_at IS NULL
        )
        AND deleted_at IS NULL
    );

-- Policy: Users can update their own profile
-- Logic: current_user_id() must match the user being updated
CREATE POLICY users_update_policy ON auth.users
    FOR UPDATE
    USING (
        id = current_user_id()
        AND deleted_at IS NULL
    );

-- Policy: Only family owners can delete users (soft delete)
-- Logic: current_user_id() must be the owner of the family
CREATE POLICY users_delete_policy ON auth.users
    FOR DELETE
    USING (
        family_id IN (
            SELECT id
            FROM auth.families
            WHERE owner_id = current_user_id()
              AND deleted_at IS NULL
        )
    );

-- Policy: Authenticated users can create their own user record (during registration)
-- Logic: current_user_id() must be set
CREATE POLICY users_insert_policy ON auth.users
    FOR INSERT
    WITH CHECK (
        current_user_id() IS NOT NULL
    );


-- ========================================
-- RLS POLICIES FOR auth.family_member_invitations
-- ========================================

-- Policy: Users can read invitations for their families
-- Logic: current_user_id() must belong to the family
CREATE POLICY invitations_select_policy ON auth.family_member_invitations
    FOR SELECT
    USING (
        family_id IN (
            SELECT family_id
            FROM auth.users
            WHERE id = current_user_id()
              AND deleted_at IS NULL
        )
    );

-- Policy: Only family owners and admins can create invitations
-- Logic: current_user_id() must be owner or admin of the family
CREATE POLICY invitations_insert_policy ON auth.family_member_invitations
    FOR INSERT
    WITH CHECK (
        family_id IN (
            SELECT family_id
            FROM auth.users
            WHERE id = current_user_id()
              AND deleted_at IS NULL
              AND role IN ('Owner', 'Admin')
        )
    );

-- Policy: Only family owners and admins can update invitations (e.g., cancel)
-- Logic: current_user_id() must be owner or admin of the family
CREATE POLICY invitations_update_policy ON auth.family_member_invitations
    FOR UPDATE
    USING (
        family_id IN (
            SELECT family_id
            FROM auth.users
            WHERE id = current_user_id()
              AND deleted_at IS NULL
              AND role IN ('Owner', 'Admin')
        )
    );

-- Policy: Only family owners and admins can delete invitations
-- Logic: current_user_id() must be owner or admin of the family
CREATE POLICY invitations_delete_policy ON auth.family_member_invitations
    FOR DELETE
    USING (
        family_id IN (
            SELECT family_id
            FROM auth.users
            WHERE id = current_user_id()
              AND deleted_at IS NULL
              AND role IN ('Owner', 'Admin')
        )
    );


-- ========================================
-- VERIFICATION QUERIES (for testing)
-- ========================================

-- To test RLS policies, use these queries:
--
-- 1. Set user context:
--    SELECT set_config('app.current_user_id', '<user-uuid>', false);
--
-- 2. Verify isolation:
--    SELECT * FROM auth.families;
--    -- Should only return families for the current user
--
-- 3. Test cross-family access:
--    SELECT * FROM auth.users WHERE family_id = '<other-family-uuid>';
--    -- Should return empty result set
--
-- 4. Reset context:
--    SELECT set_config('app.current_user_id', NULL, false);


-- Log completion
DO $$
BEGIN
    RAISE NOTICE 'Row-Level Security (RLS) policies enabled successfully';
    RAISE NOTICE 'Tables protected: families, users, family_member_invitations';
    RAISE NOTICE 'Multi-tenant isolation is now enforced at the database level';
END $$;
