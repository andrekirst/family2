# ADR-009: Modular Middleware Composition Pattern

**Status:** Accepted
**Date:** 2026-01-12
**Deciders:** Andre Kirst (with Claude Code AI)
**Tags:** middleware, asp-net-core, modular-monolith, rls, security, multi-tenancy
**Related ADRs:** [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-005](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md), [ADR-007](ADR-007-FAMILY-DBCONTEXT-SEPARATION-STRATEGY.md)
**Issue:** #76

## Context

Family Hub is a **modular monolith** (per [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md)) where each bounded context may require custom middleware. The most critical middleware requirement is **PostgreSQL Row-Level Security (RLS)** context, which must be set for every authenticated request before database queries execute.

### Problem Statement

1. **Module-Specific Middleware**: Each module may need its own middleware (Auth needs RLS, Family may need family context)
2. **Execution Order**: Middleware ordering is critical—RLS context must be set after authentication
3. **Discoverability**: Module middleware should be easy to find and configure
4. **Future Extensibility**: Pattern must scale as modules are added (Calendar, Task, Shopping, etc.)

### Technology Stack

- **ASP.NET Core 10**: Web framework
- **PostgreSQL 16**: Database with RLS
- **Zitadel**: OAuth provider (JWT tokens)
- **Hot Chocolate**: GraphQL server

### Security Requirements

PostgreSQL RLS policies require a session variable (`app.current_user_id`) to be set for every request:

```sql
-- RLS policy example
CREATE POLICY user_isolation_policy ON auth.users
    USING (id = current_setting('app.current_user_id', true)::uuid);
```

Without proper middleware, RLS cannot function, exposing data isolation vulnerabilities.

## Decision

**Implement a modular middleware composition pattern using extension methods (`UseAuthModule()`, `UseFamilyModule()`) that encapsulate module-specific middleware registration with explicit ordering.**

### Pattern Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ ASP.NET Core Middleware Pipeline                                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Request ──▶ [Exception Handler]                                            │
│              │                                                              │
│              ▼                                                              │
│         [HTTPS Redirection]                                                 │
│              │                                                              │
│              ▼                                                              │
│         [CORS Policy]                                                       │
│              │                                                              │
│              ▼                                                              │
│         [Rate Limiting]                                                     │
│              │                                                              │
│              ▼                                                              │
│         [Authentication]  ◀── Validates JWT, populates ClaimsPrincipal      │
│              │                                                              │
│              ▼                                                              │
│         [Authorization]   ◀── Validates authorization policies              │
│              │                                                              │
│              ▼                                                              │
│     ┌────────────────────┐                                                  │
│     │ UseAuthModule()    │ ◀── Sets PostgreSQL RLS session variable         │
│     │ • RLS Middleware   │                                                  │
│     └────────────────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│     ┌────────────────────┐                                                  │
│     │ UseFamilyModule()  │ ◀── Placeholder for future Family middleware     │
│     │ • (Reserved)       │                                                  │
│     └────────────────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│         [MapGraphQL()]    ◀── GraphQL endpoint                              │
│              │                                                              │
│  Response ◀──┘                                                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### UseAuthModule Implementation

```csharp
/// <summary>
/// Registers Auth module middleware in the ASP.NET Core pipeline.
///
/// EXECUTION ORDER:
/// This method MUST be called AFTER UseAuthentication() and UseAuthorization()
/// because it relies on the authenticated user claims to set the RLS context.
/// </summary>
public static IApplicationBuilder UseAuthModule(this IApplicationBuilder app)
{
    // PostgreSQL RLS context - sets session variable for Row-Level Security
    app.UseMiddleware<PostgresRlsContextMiddleware>();

    return app;
}
```

### PostgresRlsContextMiddleware Implementation

```csharp
/// <summary>
/// Middleware that sets the PostgreSQL session variable 'app.current_user_id'
/// for Row-Level Security (RLS) policies.
///
/// EXECUTION ORDER:
/// 1. Authentication middleware populates User (ClaimsPrincipal)
/// 2. This middleware extracts user ID from JWT claims
/// 3. Sets PostgreSQL session variable for RLS policies
/// 4. GraphQL/MediatR request processing occurs
/// 5. PostgreSQL enforces RLS based on session variable
///
/// SECURITY:
/// - RLS policies will deny access if current_user_id() returns NULL (unauthenticated)
/// - SQL injection prevented by parameterized queries
/// - Transaction-scoped variables prevent cross-request leakage
///
/// PERFORMANCE:
/// - One additional SQL command per request (~1ms overhead)
/// </summary>
public class PostgresRlsContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PostgresRlsContextMiddleware> _logger;

    public PostgresRlsContextMiddleware(
        RequestDelegate next,
        ILogger<PostgresRlsContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AuthDbContext dbContext)
    {
        // Extract user ID from JWT claims (if authenticated)
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User?.FindFirst("sub")?.Value; // Zitadel uses 'sub' claim

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            try
            {
                var connection = dbContext.Database.GetDbConnection();

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // Set the session variable for RLS policies
                // The 'true' parameter makes it transaction-scoped (cleared after transaction)
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT set_config('app.current_user_id', @userId, true)";
                cmd.Parameters.Add(new NpgsqlParameter("@userId", userId.ToString()));

                await cmd.ExecuteNonQueryAsync();

                _logger.LogDebug(
                    "PostgreSQL RLS context set for user {UserId}",
                    userId);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                // RLS will deny access if context not set (fail-secure)
                _logger.LogError(
                    ex,
                    "Failed to set PostgreSQL RLS context for user {UserId}",
                    userId);
            }
        }

        await _next(context);
    }
}
```

### UseFamilyModule Implementation

```csharp
/// <summary>
/// Registers Family module middleware in the ASP.NET Core pipeline.
/// Currently a placeholder for future Family-specific middleware.
///
/// EXECUTION ORDER:
/// This method MUST be called AFTER UseAuthModule() because Family operations
/// depend on authenticated user context being established.
/// </summary>
public static IApplicationBuilder UseFamilyModule(this IApplicationBuilder app)
{
    // Placeholder for future Family-specific middleware:
    // - FamilyContextMiddleware (resolve current family from route/header)
    // - FamilyPermissionMiddleware (validate family-level permissions)
    // - FamilyRateLimitingMiddleware (family-specific rate limits)

    return app;
}
```

### Program.cs Pipeline Configuration

```csharp
var app = builder.Build();

// Standard ASP.NET Core middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

// Rate Limiting
app.UseIpRateLimiting();

// Authentication and Authorization (MUST come before module middleware)
app.UseAuthentication();
app.UseAuthorization();

// Module middleware (order matters!)
app.UseAuthModule();      // Sets RLS context from auth claims
app.UseFamilyModule();    // Reserved for family-specific middleware

// GraphQL endpoint
app.MapGraphQL();

// Health checks
app.MapHealthChecks("/health", new HealthCheckOptions { ... });
```

## Rationale

### Why Extension Methods

| Approach | Pros | Cons |
|----------|------|------|
| **Extension Methods** | Discoverable, chainable, explicit | None significant |
| Attribute-based | Declarative | Hard to control ordering |
| Convention-based | Less code | Implicit, magic behavior |
| Manual registration | Full control | Verbose, easy to forget |

**Decision**: Extension methods provide the best balance of discoverability, explicit ordering, and modularity.

### Why Transaction-Scoped Session Variables

PostgreSQL `set_config` with `true` for the `is_local` parameter:

```sql
SELECT set_config('app.current_user_id', '12345', true);
--                                              ^^^^
--                                              transaction-scoped
```

Benefits:

1. **No Cross-Request Leakage**: Variable automatically cleared after transaction
2. **No Cleanup Required**: No need for finally blocks or middleware to reset
3. **Connection Pool Safe**: Works correctly with connection pooling

### Why Fail-Secure on RLS Errors

If RLS context fails to set:

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to set PostgreSQL RLS context...");
    // Continue request - RLS will deny access
}
```

RLS policies treat NULL `current_user_id` as unauthorized:

```sql
-- Returns FALSE when current_user_id is NULL
USING (id = current_setting('app.current_user_id', true)::uuid)
```

This is **fail-secure**: errors result in denied access, not data exposure.

### Why Module-Specific Middleware Files

Each module owns its middleware, keeping related code together:

```
FamilyHub.Modules.Auth/
├── Infrastructure/
│   └── Middleware/
│       └── PostgresRlsContextMiddleware.cs  ← Auth module owns RLS middleware
└── AuthModuleServiceRegistration.cs         ← UseAuthModule() extension

FamilyHub.Modules.Family/
├── Infrastructure/
│   └── Middleware/
│       └── (future middleware here)
└── FamilyModuleServiceRegistration.cs       ← UseFamilyModule() extension
```

## Alternatives Considered

### Alternative 1: Global Middleware

**Approach**: Single middleware handles all modules.

```csharp
app.UseMiddleware<GlobalModuleMiddleware>();
```

**Rejected Because**:

- Violates single responsibility
- Hard to maintain as modules grow
- Cannot control per-module ordering

### Alternative 2: Action Filters

**Approach**: Use MVC action filters or GraphQL interceptors.

```csharp
[RlsContext]
public class UserQueries { ... }
```

**Rejected Because**:

- Doesn't work for all endpoints (health checks, etc.)
- Harder to ensure execution before DbContext usage
- Less explicit than middleware

### Alternative 3: DbContext Interceptors

**Approach**: Set RLS context in EF Core interceptor.

```csharp
public class RlsInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(...)
    {
        // Set RLS context before each query
    }
}
```

**Rejected Because**:

- Runs on every query (performance overhead)
- Harder to access HttpContext in interceptor
- Less intuitive than middleware approach

## Consequences

### Positive

1. **Explicit Ordering**: Pipeline order clearly visible in Program.cs
2. **Module Encapsulation**: Each module manages its own middleware
3. **Discoverability**: Extension methods easy to find via IntelliSense
4. **Security**: RLS context properly set before any database access
5. **Extensibility**: Pattern scales to additional modules

### Negative

1. **Manual Ordering**: Developer must know correct middleware order
2. **Overhead**: ~1ms per request for RLS context setting
3. **DbContext Coupling**: RLS middleware depends on AuthDbContext

### Mitigation Strategies

| Risk | Mitigation |
|------|------------|
| Wrong ordering | XML documentation, code review, integration tests |
| Performance | Minimal overhead (~1ms), could cache connections |
| Coupling | Abstract to IDbConnection if multiple contexts need RLS |

## Implementation

### Files Created/Modified

| File | Purpose |
|------|---------|
| `Modules/FamilyHub.Modules.Auth/Infrastructure/Middleware/PostgresRlsContextMiddleware.cs` | RLS context middleware |
| `Modules/FamilyHub.Modules.Auth/AuthModuleServiceRegistration.cs` | UseAuthModule() extension |
| `Modules/FamilyHub.Modules.Family/FamilyModuleServiceRegistration.cs` | UseFamilyModule() extension |
| `FamilyHub.Api/Program.cs` | Middleware pipeline configuration |

### Verification

1. **Build**: `dotnet build` completes without errors
2. **RLS Test**: Authenticated queries return only user's data
3. **Unauthenticated Test**: No data returned for unauthenticated requests
4. **Logging**: Debug logs show RLS context being set
5. **Performance**: Request latency increase < 2ms

### Testing RLS Context

```csharp
[Fact]
public async Task AuthenticatedUser_ShouldOnlySeeOwnData()
{
    // Arrange: Create two users with families
    var user1 = await CreateUserWithFamily("user1");
    var user2 = await CreateUserWithFamily("user2");

    // Act: Query as user1
    var result = await ExecuteGraphQL(
        "query { families { id } }",
        authenticatedAs: user1);

    // Assert: Only user1's family returned
    result.Families.Should().HaveCount(1);
    result.Families[0].Id.Should().Be(user1.FamilyId);
}
```

## Related Decisions

- [ADR-001: Modular Monolith First](ADR-001-MODULAR-MONOLITH-FIRST.md) - Module architecture
- [ADR-005: Family Module Extraction Pattern](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md) - Module structure
- [ADR-007: Family DbContext Separation Strategy](ADR-007-FAMILY-DBCONTEXT-SEPARATION-STRATEGY.md) - Schema separation enables RLS

## Future Work

- **FamilyContextMiddleware**: Resolve current family from JWT claim or route parameter
- **PermissionMiddleware**: Validate family-level permissions (Owner, Admin, Member)
- **AuditMiddleware**: Log all data access for compliance
- **Module Discovery**: Automatic middleware registration via attributes (Phase 5+)

## References

- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware)
- [PostgreSQL Row-Level Security](https://www.postgresql.org/docs/16/ddl-rowsecurity.html)
- [Multi-Tenancy Strategy](multi-tenancy-strategy.md)

---

**Decision**: Implement modular middleware composition using extension methods (`UseAuthModule()`, `UseFamilyModule()`) with explicit ordering in Program.cs. PostgreSQL RLS context is set via `PostgresRlsContextMiddleware` using transaction-scoped session variables, ensuring fail-secure behavior when errors occur.
