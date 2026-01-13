# Middleware Execution Order

This document describes the ASP.NET Core middleware pipeline order for Family Hub.

## Overview

Middleware executes in order during the request phase (top-to-bottom) and in reverse order during the response phase (bottom-to-top). Understanding this order is critical for proper authentication, authorization, and data access.

## Pipeline Order

```
Request  →  ┌──────────────────────────────────┐  ← Response
            │  1. Developer Exception Page     │  (Dev only)
            │  2. HTTPS Redirection            │
            │  3. CORS                         │
            │  4. Rate Limiting                │
            │  5. Authentication               │  ← Validates JWT
            │  6. Authorization                │  ← Validates policies
            │  7a. UseAuthModule()             │  ← RLS context
            │  7b. UseFamilyModule()           │  ← (future)
            │  8. Endpoints (GraphQL)          │
            └──────────────────────────────────┘
```

## Execution Order Table

| Order | Middleware                  | Module   | Purpose                                        |
| ----- | --------------------------- | -------- | ---------------------------------------------- |
| 1     | UseDeveloperExceptionPage() | Api      | Development exception handling (dev only)      |
| 2     | UseHttpsRedirection()       | Api      | HTTPS redirect                                 |
| 3     | UseCors()                   | Api      | CORS policy for Angular frontend               |
| 4     | UseIpRateLimiting()         | Api      | Rate limiting (AspNetCoreRateLimit)            |
| 5     | UseAuthentication()         | Api      | JWT validation, populates HttpContext.User     |
| 6     | UseAuthorization()          | Api      | Authorization policy validation                |
| 7a    | UseAuthModule()             | Auth     | PostgreSQL RLS context (app.current_user_id)   |
| 7b    | UseFamilyModule()           | Family   | Future expansion (currently no-op)             |
| 8     | MapGraphQL()                | Api      | GraphQL endpoint                               |
| 9     | MapHealthChecks()           | Api      | Health check endpoints (/health, /health/\*)   |

## Module Middleware Details

### UseAuthModule()

**Location:** `FamilyHub.Modules.Auth.AuthModuleServiceRegistration`

**Purpose:** Registers Auth module-specific middleware in the ASP.NET Core pipeline.

**Current middleware:**

- `PostgresRlsContextMiddleware` - Sets PostgreSQL session variable `app.current_user_id` for Row-Level Security

**Dependencies:**

- MUST run after `UseAuthentication()` - needs `HttpContext.User` populated with JWT claims
- MUST run after `UseAuthorization()` - ensures authorization policies are applied before RLS context is set

**Source file:** `src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Middleware/PostgresRlsContextMiddleware.cs`

### UseFamilyModule()

**Location:** `FamilyHub.Modules.Family.FamilyModuleServiceRegistration`

**Purpose:** Registers Family module-specific middleware (placeholder for future expansion).

**Current middleware:** None (placeholder)

**Planned middleware candidates:**

- Family context resolution (set current family from JWT or route)
- Family permission validation middleware
- Family-specific rate limiting

**Dependencies:**

- MUST run after `UseAuthModule()` - Family operations depend on authenticated user context

## PostgreSQL RLS Context Middleware

### How It Works

1. **Request arrives** → Authentication middleware validates JWT and populates `HttpContext.User`
2. **PostgresRlsContextMiddleware executes** → Extracts user ID from JWT `sub` claim
3. **Sets PostgreSQL session variable** → `SELECT set_config('app.current_user_id', @userId, true)`
4. **GraphQL request processing** → EF Core queries execute with RLS policies active
5. **PostgreSQL enforces RLS** → Queries automatically filtered by `current_setting('app.current_user_id')`

### Security Considerations

- **Transaction-scoped:** The `true` parameter in `set_config` ensures the variable is cleared after each transaction
- **Fail-secure:** If user ID extraction fails, RLS policies deny access (no data leakage)
- **SQL injection prevention:** User ID is passed as a parameterized query, not string concatenation
- **Unauthenticated requests:** RLS policies handle NULL user_id gracefully (return empty results)

### Performance

- **Overhead:** ~1ms per request (one additional SQL command)
- **Benefit:** Eliminates need for explicit `WHERE family_id = ...` filters in application code
- **Indexing:** RLS policies use indexed columns (family_id, user_id) for efficient filtering

## Adding New Module Middleware

When adding middleware to a new or existing module:

### 1. Create Middleware Class

```csharp
// Location: Modules/{ModuleName}/Infrastructure/Middleware/{MiddlewareName}.cs
namespace FamilyHub.Modules.{ModuleName}.Infrastructure.Middleware;

public class {MiddlewareName}
{
    private readonly RequestDelegate _next;
    private readonly ILogger<{MiddlewareName}> _logger;

    public {MiddlewareName}(RequestDelegate next, ILogger<{MiddlewareName}> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Middleware logic here
        await _next(context);
    }
}
```

### 2. Register in Module Extension

```csharp
// Location: Modules/{ModuleName}/{ModuleName}ModuleServiceRegistration.cs
public static IApplicationBuilder Use{ModuleName}Module(this IApplicationBuilder app)
{
    app.UseMiddleware<Infrastructure.Middleware.{MiddlewareName}>();
    return app;
}
```

### 3. Update Program.cs

```csharp
// Add after UseAuthentication/UseAuthorization
app.UseAuthModule();
app.UseFamilyModule();
app.Use{NewModuleName}Module(); // New module middleware
```

### 4. Update This Documentation

Add the new middleware to the execution order table and create a section describing its purpose.

## Related Documentation

- [WORKFLOWS.md](./WORKFLOWS.md) - Development workflows
- [CODING_STANDARDS.md](./CODING_STANDARDS.md) - Code style guidelines
- [ADR-001: Modular Monolith First](../architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) - Architecture decision

---

**Last Updated:** 2026-01-12
**Related Issue:** #62 - Implement Modular Middleware Composition
