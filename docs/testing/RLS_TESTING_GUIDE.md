# Row-Level Security (RLS) Testing Guide

**Purpose:** This guide explains how to manually test PostgreSQL Row-Level Security (RLS) policies to verify multi-tenant data isolation.

**Related:** Issue #44 - Implement RLS Policies for Multi-Tenant Isolation

---

## Prerequisites

1. Docker Compose running (PostgreSQL, Zitadel)
2. Backend API running (`dotnet run --project src/api/FamilyHub.Api`)
3. At least 2 test users in different families
4. `psql` CLI tool installed

---

## Test Scenario: Verify Family Data Isolation

### Setup

1. **Start Services:**

   ```bash
   cd infrastructure/docker
   docker-compose up -d
   cd ../../src/api
   dotnet run --project FamilyHub.Api
   ```

2. **Create Two Test Users:**
   - User A: Register via GraphQL, create Family A
   - User B: Register via GraphQL, create Family B

### Manual Test Steps

#### Step 1: Verify RLS Policies Exist

```bash
# Connect to PostgreSQL
psql -h localhost -U familyhub -d familyhub

# Check RLS is enabled on tables
SELECT tablename, rowsecurity
FROM pg_tables
WHERE schemaname = 'auth'
  AND tablename IN ('families', 'users', 'family_member_invitations');

# Expected Output:
# tablename                     | rowsecurity
# ------------------------------+-------------
# families                      | t
# users                         | t
# family_member_invitations     | t

# List RLS policies
SELECT schemaname, tablename, policyname, cmd, qual
FROM pg_policies
WHERE schemaname = 'auth';

# Should show policies like:
# - family_select_policy (SELECT)
# - users_select_policy (SELECT)
# - invitations_select_policy (SELECT)
# - etc.
```

#### Step 2: Test Without User Context (Unauthenticated)

```bash
# In psql:

# Try to select families without setting user context
SELECT * FROM auth.families;

# Expected: Empty result (no rows)
# Reason: current_user_id() returns NULL, RLS denies access
```

#### Step 3: Test With User A Context

```bash
# Set User A's ID (replace with actual GUID from database)
SELECT set_config('app.current_user_id', '<USER_A_UUID>', true);

# Query families
SELECT id, name, owner_id FROM auth.families;

# Expected: Only Family A visible

# Try to query Family B directly
SELECT * FROM auth.families WHERE id = '<FAMILY_B_UUID>';

# Expected: Empty result (RLS blocks cross-family access)
```

#### Step 4: Test With User B Context

```bash
# Set User B's ID
SELECT set_config('app.current_user_id', '<USER_B_UUID>', true);

# Query families
SELECT id, name, owner_id FROM auth.families;

# Expected: Only Family B visible

# Try to query Family A directly
SELECT * FROM auth.families WHERE id = '<FAMILY_A_UUID>';

# Expected: Empty result (RLS blocks cross-family access)
```

#### Step 5: Test GraphQL Multi-Tenant Isolation

```graphql
# User A's GraphQL request (with User A's JWT token)
query {
  families {
    id
    name
    ownerId
  }
}

# Expected Response: Only Family A returned

# User B's GraphQL request (with User B's JWT token)
query {
  families {
    id
    name
    ownerId
  }
}

# Expected Response: Only Family B returned
```

#### Step 6: Verify Users Can't See Other Families' Users

```bash
# In psql, set User A context:
SELECT set_config('app.current_user_id', '<USER_A_UUID>', true);

# Query users
SELECT id, email, family_id FROM auth.users;

# Expected: Only users in Family A visible

# Try to query User B directly
SELECT * FROM auth.users WHERE id = '<USER_B_UUID>';

# Expected: Empty result (different family)
```

---

## Integration Test Example (Future Implementation)

```csharp
[Fact]
public async Task RLS_PreventsCrossFamilyDataAccess()
{
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    // Create two families with different owners
    var userA = await CreateTestUser("userA@test.com");
    var familyA = await CreateTestFamily("Family A", userA.Id);

    var userB = await CreateTestUser("userB@test.com");
    var familyB = await CreateTestFamily("Family B", userB.Id);

    // Act: Set User A context and query families
    await SetPostgresUserContext(dbContext, userA.Id);
    var familiesForUserA = await dbContext.Families.ToListAsync();

    // Assert: User A can only see Family A
    familiesForUserA.Should().HaveCount(1);
    familiesForUserA.Single().Id.Should().Be(familyA.Id);

    // Act: Set User B context and query families
    await SetPostgresUserContext(dbContext, userB.Id);
    var familiesForUserB = await dbContext.Families.ToListAsync();

    // Assert: User B can only see Family B
    familiesForUserB.Should().HaveCount(1);
    familiesForUserB.Single().Id.Should().Be(familyB.Id);
}

private async Task SetPostgresUserContext(AuthDbContext dbContext, UserId userId)
{
    var connection = dbContext.Database.GetDbConnection();
    await using var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT set_config('app.current_user_id', @userId, true)";
    cmd.Parameters.Add(new NpgsqlParameter("@userId", userId.Value.ToString()));
    await cmd.ExecuteNonQueryAsync();
}
```

---

## Common Issues

### Issue 1: RLS Policies Not Loaded

**Symptom:** All users can see all families

**Solution:**

```bash
# Rebuild Docker Compose to load init scripts
cd infrastructure/docker
docker-compose down -v  # WARNING: Deletes all data
docker-compose up -d

# Or manually apply RLS script
psql -h localhost -U familyhub -d familyhub -f postgres/init-scripts/002-enable-rls-policies.sql
```

### Issue 2: Middleware Not Setting User Context

**Symptom:** All queries return empty results even for authenticated users

**Verification:**

```bash
# Check Program.cs has middleware registered AFTER UseAuthentication()
# Check middleware logs for "PostgreSQL RLS context set for user"
```

**Solution:**
Ensure `app.UsePostgresContext()` is called after `app.UseAuthentication()` in Program.cs

### Issue 3: current_user_id() Returns NULL

**Symptom:** Authenticated users get empty results

**Debugging:**

```bash
# In psql:
SELECT current_setting('app.current_user_id', true);

# Should return the user's UUID
# If NULL, middleware is not setting context correctly
```

---

## Security Verification Checklist

- [ ] User A cannot query Family B's data
- [ ] User B cannot query Family A's data
- [ ] Unauthenticated requests return empty results (not errors)
- [ ] Family owners can update their own families
- [ ] Family owners cannot update other families
- [ ] Users can only see family members in their own family
- [ ] Only family owners/admins can create/manage invitations
- [ ] PostgreSQL logs show no RLS policy violations

---

## Performance Testing

RLS policies use indexed columns (family_id, user_id), so performance impact should be minimal (<10ms per query).

**Test Query Performance:**

```bash
# In psql:
EXPLAIN ANALYZE
SELECT * FROM auth.families
WHERE id IN (
    SELECT family_id FROM auth.users WHERE id = current_user_id()
);

# Check execution time and index usage
```

---

## References

- **RLS SQL Script:** `/infrastructure/docker/postgres/init-scripts/002-enable-rls-policies.sql`
- **Middleware:** `/src/api/FamilyHub.Api/Middleware/PostgresContextMiddleware.cs`
- **PostgreSQL RLS Docs:** https://www.postgresql.org/docs/16/ddl-rowsecurity.html
- **Phase 0 Plan:** `/docs/phase-0/COMPLETION_REPORT.md`

---

**Last Updated:** 2026-01-09
**Status:** RLS Implemented (Manual Testing Required)
