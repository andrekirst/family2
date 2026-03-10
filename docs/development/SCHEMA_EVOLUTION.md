# Schema Evolution Strategy

## Overview

Family Hub uses DbUp for database migrations with PostgreSQL. This document defines the coordinated release strategy for schema changes that require synchronized backend deployment.

## Migration Workflow

### Adding a New Column (Non-Breaking)

1. Create migration script: `NNNN-add-column-to-table.sql`
2. Use `ALTER TABLE ... ADD COLUMN ... DEFAULT ...` (always provide a default)
3. Deploy migration first, then deploy code that uses the column
4. This is a **two-phase deployment**: schema change → code change

### Removing a Column (Breaking)

1. **Phase 1:** Deploy code that stops reading/writing the column
2. **Phase 2:** Create migration to drop the column
3. Never drop a column that active code still references

### Renaming a Column

1. **Phase 1:** Add new column with migration, deploy code that writes to both old and new
2. **Phase 2:** Backfill data from old to new column
3. **Phase 3:** Deploy code that reads only from new column
4. **Phase 4:** Drop old column

### Adding a New Table

1. Create migration with full table definition including indexes and constraints
2. Deploy migration and code together (new table has no existing consumers)

## Migration Naming Convention

```
NNNN-<action>-<object>-<context>.sql
```

Examples:

- `0015-add-row-version-to-families.sql`
- `0016-create-audit-events-table.sql`
- `0017-add-index-on-calendar-events-family-id.sql`

## RLS Considerations

All new tables holding family-scoped data must include:

1. A `family_id` column
2. An RLS policy: `CREATE POLICY ... USING (family_id = current_setting('app.current_family_id')::uuid)`
3. `ALTER TABLE ... ENABLE ROW LEVEL SECURITY`

## Rollback Strategy

- Each migration should be reversible where possible
- Keep a corresponding `NNNN-rollback-<description>.sql` for complex migrations
- For data-destructive changes, take a logical backup before applying

## Environment Promotion

```
Development → Staging → Production
```

- Migrations run automatically on app startup via DbUp
- Staging validates migration against production-like data volumes
- Production deployments happen during low-traffic windows for major schema changes
