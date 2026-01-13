-- DataLoader Performance Test Data Seed Script
-- Creates families, users, and invitations for k6 benchmarking
-- Idempotent: Safe to run multiple times (uses ON CONFLICT DO NOTHING)
--
-- Data Structure:
--   - 5 families with deterministic UUIDs (10000000-0000-0000-0000-00000000000X)
--   - 5 owner users with deterministic UUIDs (00000000-0000-0000-0000-00000000000X)
--   - 50 additional members per family (250 total)
--   - 20 pending invitations per family (100 total)
--
-- Usage:
--   psql $DATABASE_URL -f dataloader-test-data.sql
--   # or in CI:
--   PGPASSWORD=$POSTGRES_PASSWORD psql -h localhost -U postgres -d familyhub -f dataloader-test-data.sql

BEGIN;

-- ============================================
-- Step 1: Create test families
-- ============================================

INSERT INTO auth.families (id, name, owner_id, created_at, updated_at)
VALUES
    ('10000000-0000-0000-0000-000000000001', 'Performance Test Family 1', '00000000-0000-0000-0000-000000000001', NOW(), NOW()),
    ('10000000-0000-0000-0000-000000000002', 'Performance Test Family 2', '00000000-0000-0000-0000-000000000002', NOW(), NOW()),
    ('10000000-0000-0000-0000-000000000003', 'Performance Test Family 3', '00000000-0000-0000-0000-000000000003', NOW(), NOW()),
    ('10000000-0000-0000-0000-000000000004', 'Performance Test Family 4', '00000000-0000-0000-0000-000000000004', NOW(), NOW()),
    ('10000000-0000-0000-0000-000000000005', 'Performance Test Family 5', '00000000-0000-0000-0000-000000000005', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- Step 2: Create owner users (referenced in k6 testUsers config)
-- These are the users that k6 will use via X-Test-User-Id header
-- ============================================

INSERT INTO auth.users (id, email, external_user_id, external_provider, family_id, role, created_at, updated_at)
VALUES
    ('00000000-0000-0000-0000-000000000001', 'testuser1@test.local', 'ext-perf-test-1', 'test', '10000000-0000-0000-0000-000000000001', 'Owner', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000002', 'testuser2@test.local', 'ext-perf-test-2', 'test', '10000000-0000-0000-0000-000000000002', 'Owner', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000003', 'testuser3@test.local', 'ext-perf-test-3', 'test', '10000000-0000-0000-0000-000000000003', 'Owner', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000004', 'testuser4@test.local', 'ext-perf-test-4', 'test', '10000000-0000-0000-0000-000000000004', 'Owner', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000005', 'testuser5@test.local', 'ext-perf-test-5', 'test', '10000000-0000-0000-0000-000000000005', 'Owner', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- Step 3: Create 50 additional members per family (250 total)
-- Uses DO block with generate_series for bulk creation
-- ============================================

DO $$
DECLARE
    family_uuid uuid;
    family_idx int;
    member_idx int;
    member_uuid uuid;
BEGIN
    FOR family_idx IN 1..5 LOOP
        family_uuid := ('10000000-0000-0000-0000-00000000000' || family_idx)::uuid;

        FOR member_idx IN 1..50 LOOP
            -- Generate deterministic UUID for reproducibility
            -- Format: 20000000-FFFF-0000-0000-00000000MMNN
            -- Where FF = family_idx, MM = member_idx high byte, NN = member_idx low byte
            member_uuid := ('20000000-000' || family_idx || '-0000-0000-0000' || LPAD(((family_idx - 1) * 50 + member_idx)::text, 8, '0'))::uuid;

            INSERT INTO auth.users (id, email, external_user_id, external_provider, family_id, role, created_at, updated_at)
            VALUES (
                member_uuid,
                'member-' || family_idx || '-' || member_idx || '@test.local',
                'ext-member-' || family_idx || '-' || member_idx,
                'test',
                family_uuid,
                'Member',
                NOW(),
                NOW()
            )
            ON CONFLICT (id) DO NOTHING;
        END LOOP;
    END LOOP;
END $$;

-- ============================================
-- Step 4: Create 20 invitations per family (100 total)
-- ============================================

DO $$
DECLARE
    family_uuid uuid;
    owner_uuid uuid;
    family_idx int;
    inv_idx int;
    invitation_uuid uuid;
    invitation_token text;
BEGIN
    FOR family_idx IN 1..5 LOOP
        family_uuid := ('10000000-0000-0000-0000-00000000000' || family_idx)::uuid;
        owner_uuid := ('00000000-0000-0000-0000-00000000000' || family_idx)::uuid;

        FOR inv_idx IN 1..20 LOOP
            -- Generate deterministic UUID for reproducibility
            -- Format: 30000000-FFFF-0000-0000-0000000000II
            invitation_uuid := ('30000000-000' || family_idx || '-0000-0000-0000' || LPAD(((family_idx - 1) * 20 + inv_idx)::text, 8, '0'))::uuid;

            -- Generate unique token (SHA256-based for determinism)
            invitation_token := ENCODE(SHA256(('perf-test-token-' || family_idx || '-' || inv_idx)::bytea), 'hex');

            INSERT INTO auth.family_member_invitations (
                invitation_id, family_id, email, role, status, invited_by_user_id,
                message, display_code, token, expires_at, created_at, updated_at
            )
            VALUES (
                invitation_uuid,
                family_uuid,
                'invite-' || family_idx || '-' || inv_idx || '@test.local',
                'Member',
                'Pending',
                owner_uuid,
                'Performance test invitation ' || inv_idx,
                UPPER(SUBSTRING(MD5(family_idx::text || '-' || inv_idx::text), 1, 8)),
                SUBSTRING(invitation_token, 1, 64),
                NOW() + INTERVAL '14 days',
                NOW(),
                NOW()
            )
            ON CONFLICT (invitation_id) DO NOTHING;
        END LOOP;
    END LOOP;
END $$;

-- ============================================
-- Step 5: Verify seeded data
-- ============================================

SELECT 'Families created: ' || COUNT(*) AS result FROM auth.families WHERE name LIKE 'Performance Test Family %';
SELECT 'Owner users created: ' || COUNT(*) AS result FROM auth.users WHERE email LIKE 'testuser%@test.local';
SELECT 'Member users created: ' || COUNT(*) AS result FROM auth.users WHERE email LIKE 'member-%@test.local';
SELECT 'Invitations created: ' || COUNT(*) AS result FROM auth.family_member_invitations WHERE email LIKE 'invite-%@test.local';

-- Show sample data for verification
SELECT 'Sample family: ' || id || ' - ' || name AS result FROM auth.families WHERE name LIKE 'Performance Test Family %' LIMIT 1;
SELECT 'Sample user: ' || id || ' - ' || email AS result FROM auth.users WHERE email LIKE 'testuser%@test.local' LIMIT 1;

COMMIT;

-- ============================================
-- Summary
-- ============================================
-- Total entities created:
--   - 5 families
--   - 5 owner users (for k6 authentication)
--   - 250 member users (50 per family)
--   - 100 invitations (20 per family)
--
-- Test User IDs (use these with X-Test-User-Id header):
--   - 00000000-0000-0000-0000-000000000001 (testuser1@test.local)
--   - 00000000-0000-0000-0000-000000000002 (testuser2@test.local)
--   - 00000000-0000-0000-0000-000000000003 (testuser3@test.local)
--   - 00000000-0000-0000-0000-000000000004 (testuser4@test.local)
--   - 00000000-0000-0000-0000-000000000005 (testuser5@test.local)
