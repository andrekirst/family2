# Phase 0: CHILD → MANAGED_ACCOUNT Migration - Rollback Procedures

**Epic:** #24 - Family Member Invitation System
**Migration:** Phase 0 - Terminology Update
**Date:** 2026-01-04
**Status:** ✅ COMPLETED (All 3 steps executed successfully)

---

## Overview

This document outlines rollback procedures for each step of the 3-phase CHILD → MANAGED_ACCOUNT role migration.

**Migration Strategy:**
- **Step 0.1:** Add MANAGED_ACCOUNT to C# code, keep CHILD (✅ COMPLETED)
- **Step 0.2:** Migrate data from CHILD → MANAGED_ACCOUNT (✅ COMPLETED - Verification-only)
- **Step 0.3:** Remove CHILD from C# code (✅ COMPLETED)

---

## Step 0.1: Add MANAGED_ACCOUNT Role - Rollback

**Migration File:** `20260104215155_AddManagedAccountRole.cs`
**Status:** ✅ Applied to database
**Database Changes:** None (empty migration)
**Code Changes:**
- Added `UserRoleConstants.ManagedAccountValue` constant
- Added `UserRole.ManagedAccount` static property
- Marked `UserRole.Child` as `[Obsolete]`
- Updated `ValidRoles` array to include both values

### Rollback Procedure (If Needed)

Since this migration contains NO database schema changes, rollback is simple:

```bash
# 1. Revert the EF Core migration (removes migration from __EFMigrationsHistory)
dotnet ef migrations remove \
  --context AuthDbContext \
  --project src/api/Modules/FamilyHub.Modules.Auth \
  --startup-project src/api/FamilyHub.Api

# 2. Alternatively, revert via git (recommended if migration was committed)
git revert <commit-hash>

# 3. Rebuild the project
dotnet build src/api/Modules/FamilyHub.Modules.Auth
```

**Verification:**
```bash
# Check migrations list
dotnet ef migrations list --context AuthDbContext \
  --project src/api/Modules/FamilyHub.Modules.Auth \
  --startup-project src/api/FamilyHub.Api

# Verify database migration history
docker exec -it familyhub-postgres psql -U familyhub -d familyhub \
  -c "SELECT * FROM auth.__EFMigrationsHistory ORDER BY migration_id DESC LIMIT 5;"
```

**Impact:** Zero downtime, no data loss. Application continues working normally.

---

## Step 0.2: Data Migration - Rollback

**Status:** ✅ COMPLETED (Verification-only migration)
**Migration:** `20260104215404_MigrateChildToManagedAccount`

### Future Rollback Procedure

```bash
# Rollback data migration (reverses UPDATE statements)
dotnet ef database update <previous-migration-name> \
  --context AuthDbContext \
  --project src/api/Modules/FamilyHub.Modules.Auth \
  --startup-project src/api/FamilyHub.Api

# Or manual SQL rollback (if data migration fails)
docker exec -it familyhub-postgres psql -U familyhub -d familyhub \
  -c "UPDATE auth.users SET role = 'child' WHERE role = 'managed_account';"
```

**Note:** Since user confirmed NO existing CHILD data, this migration will be a NO-OP. Rollback will also be a NO-OP.

---

## Step 0.3: Remove CHILD Enum - Rollback

**Status:** ✅ COMPLETED
**Executed:** 2026-01-04

### Future Rollback Procedure

```bash
# Revert code changes via git
git revert <commit-hash>

# Rebuild
dotnet build src/api/Modules/FamilyHub.Modules.Auth
```

**Warning:** This step removes `UserRole.Child`. If any code or database still references "child" role, rollback is critical.

---

## Emergency Procedures

### If Application Fails After Migration

1. **Check application logs:**
```bash
docker logs familyhub-api --tail 100
```

2. **Verify database state:**
```bash
docker exec -it familyhub-postgres psql -U familyhub -d familyhub \
  -c "SELECT role, COUNT(*) FROM auth.users GROUP BY role;"
```

3. **Rollback immediately:**
```bash
# Revert git commit
git revert HEAD

# Rebuild and restart
dotnet build src/api/FamilyHub.Api
docker-compose restart familyhub-api
```

### If Data Corruption Detected

**User confirmed NO existing CHILD data**, so this scenario is unlikely. However:

```bash
# Restore from backup (if available)
docker exec -i familyhub-postgres pg_restore -U familyhub -d familyhub < backup.sql

# Or point-in-time recovery (PostgreSQL 16 feature)
# See: https://www.postgresql.org/docs/16/continuous-archiving.html
```

---

## Verification Checklist

After each step, verify:

- [ ] Application starts successfully
- [ ] No errors in logs
- [ ] Database migration history correct
- [ ] Tests pass
- [ ] GraphQL schema introspection works
- [ ] No obsolete warnings in unexpected places

---

## Contact

**DBA:** Claude Code (database-administrator agent)
**Epic Owner:** See `IMPLEMENTATION_PLAN_EPIC_24.md`
**Documentation:** `/docs/migrations/`

---

_Last Updated: 2026-01-04_
_Migration Phase: ✅ ALL STEPS COMPLETED (0.1, 0.2, 0.3)_
