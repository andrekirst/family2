# SQL Design Reference Documentation

**⚠️ IMPORTANT: These SQL scripts are REFERENCE DOCUMENTATION ONLY**

These files are NOT executed in the application. They serve as design documentation created during Phase 0 planning.

## Purpose

This folder contains the original SQL migration scripts created by the database administrator agent as part of the database schema design process. While the project has transitioned to **EF Core Code-First migrations** for actual schema management, these SQL scripts remain valuable as:

### 1. Design Reference

- **Complete schema design** for all 8 modules
- **Row-Level Security (RLS) policies** - design patterns for multi-tenancy
- **Trigger logic** - automated workflows and audit trails
- **Database constraints** - validation and referential integrity rules
- **Seed data examples** - test data structure and format

### 2. PostgreSQL-Specific Features

When implementing EF Core migrations, developers can reference these scripts for:

- RLS policy syntax and patterns
- Trigger implementations
- Custom functions
- Complex constraints

These features are added to EF Core migrations using `migrationBuilder.Sql()`.

### 3. Documentation

- Comprehensive table designs
- Index strategies
- Security policies
- Performance considerations

## Folder Structure

```
sql-design/
├── README.md (this file)
├── migrations/
│   └── auth/
│       ├── 001_create_auth_schema.sql      # Table definitions
│       ├── 002_create_rls_policies.sql     # Row-Level Security
│       ├── 003_create_triggers.sql         # Automated workflows
│       └── 004_seed_data.sql               # Test data examples
└── scripts/
    └── run_migrations.sh                   # Reference script (not used)
```

## Actual Implementation

**The actual database schema is created using EF Core Code-First migrations.**

See `/database/docs/MIGRATION_STRATEGY.md` for:

- How to create EF Core migrations
- How to incorporate PostgreSQL-specific features (RLS, triggers)
- Module-specific DbContext configurations
- Migration execution strategies

## Migration from SQL to EF Core

When implementing a module, developers should:

1. **Read the SQL design** in this folder to understand:
   - Table structure and relationships
   - RLS policies needed
   - Trigger logic required
   - Seed data format

2. **Create entity classes** in the module's `Domain/` folder
   - Map to the table design
   - Use Vogen value objects for IDs and value types

3. **Configure with Fluent API** in `Persistence/Configurations/`
   - Entity mappings
   - Relationships
   - Constraints

4. **Generate EF Core migration**

   ```bash
   dotnet ef migrations add InitialCreate --context AuthDbContext
   ```

5. **Add PostgreSQL-specific features** using `migrationBuilder.Sql()`
   - Reference the RLS policies from `002_create_rls_policies.sql`
   - Reference the triggers from `003_create_triggers.sql`

## Example: Auth Module

### SQL Reference (this folder)

- `/migrations/auth/001_create_auth_schema.sql` - Table definitions
- `/migrations/auth/002_create_rls_policies.sql` - RLS policies

### EF Core Implementation (actual code)

- `/src/api/Modules/FamilyHub.Modules.Auth/Domain/User.cs` - Entity
- `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Configurations/UserConfiguration.cs` - Fluent API
- `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Migrations/20250122_InitialCreate.cs` - Migration

The EF Core migration includes:

- Auto-generated table creation (from Fluent API)
- Manual SQL for RLS policies (from reference scripts)
- Manual SQL for triggers (from reference scripts)

## Benefits of This Approach

1. **Type Safety** - EF Core migrations are C# code, compiled and type-checked
2. **Maintainability** - Changes to entities automatically update schema
3. **Testability** - Migrations can be tested in CI/CD pipelines
4. **Database Agnostic** - Can switch between PostgreSQL, SQL Server, SQLite
5. **Version Control** - Migrations are tracked in git
6. **Rollback Support** - EF Core generates `Down()` methods

## Do NOT Use These Scripts For

❌ **Running migrations** - Use EF Core migrations instead
❌ **Schema changes** - Modify entity classes and Fluent API configurations
❌ **Production deployments** - Use `dotnet ef database update` or programmatic migration

## DO Use These Scripts For

✅ **Understanding schema design** - Reference during entity creation
✅ **RLS policy patterns** - Copy into `migrationBuilder.Sql()`
✅ **Trigger logic** - Translate to `migrationBuilder.Sql()`
✅ **Documentation** - Design rationale and constraints
✅ **Seed data structure** - Test data examples

---

**Last Updated:** 2025-12-22
**Status:** Reference Documentation Only
**Official Strategy:** EF Core Code-First Migrations (see `/database/docs/MIGRATION_STRATEGY.md`)
