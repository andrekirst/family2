# Simple Architecture Guide

**Status**: Active (Phase 0)
**Date**: 2026-02-01
**Supersedes**: Modular Monolith architecture from ADR-001 v1.0

---

## Overview

Family Hub uses a **simplified single-project architecture** for Phase 0 to enable rapid development and easy maintenance. This is a deliberate departure from the originally planned modular monolith to reduce complexity during the foundational phase.

---

## Project Structure

> **Note:** Solution file is co-located with the API project at `src/FamilyHub.Api/FamilyHub.sln`.

```
repository-root/
├── src/
│   └── FamilyHub.Api/              # Single API project
│       ├── FamilyHub.sln           # Solution file (co-located)
│       ├── FamilyHub.Api.csproj
│       ├── Features/               # Feature-based organization
│       │   ├── Auth/
│       │   │   ├── Models/         # Entities and DTOs
│       │   │   ├── Services/       # Business logic
│       │   │   ├── GraphQL/        # GraphQL resolvers
│       │   │   └── Data/           # EF Core configurations
│       │   └── Family/
│       │       └── [same structure]
│       ├── Common/                 # Shared code
│       │   ├── Database/           # AppDbContext
│       │   ├── Authentication/     # JWT setup
│       │   └── Middleware/         # RLS, CORS, etc.
│       ├── Migrations/             # EF Core migrations
│       └── Program.cs              # Startup configuration
├── tests/
│   ├── FamilyHub.UnitTests/        # Unit tests
│   └── FamilyHub.IntegrationTests/ # Integration tests
└── src/frontend/family-hub-web/    # Angular frontend
```

---

## Key Simplifications

### 1. Single API Project

**Instead of**: Separate projects for SharedKernel, Auth module, Family module
**Now**: Single `FamilyHub.Api` project with feature folders

**Benefits**:

- Faster compilation
- Simpler navigation
- No cross-project dependencies
- Easier for new developers

### 2. Feature Folders

**Pattern**: Group by feature, not by layer

```
Features/Auth/
├── Models/        # User.cs, UserDto.cs
├── Services/      # AuthService.cs
├── GraphQL/       # AuthMutations.cs, AuthQueries.cs
└── Data/          # UserConfiguration.cs (EF Core)
```

**Benefits**:

- All related code in one place
- Easy to find and modify
- Clear feature boundaries

### 3. Single AppDbContext

**Instead of**: One DbContext per module (AuthDbContext, FamilyDbContext)
**Now**: Single `AppDbContext` with schema separation

```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }        // auth schema
    public DbSet<Family> Families { get; set; }   // family schema
}
```

**Benefits**:

- Standard EF Core patterns
- Foreign keys between entities
- Single migration workflow
- No cross-module abstractions needed

### 4. Removed Patterns

**Not using**:

- ❌ Vogen value objects (over-engineering)
- ❌ MediatR CQRS (unnecessary complexity)
- ❌ FluentValidation (built-in validation sufficient)
- ❌ DDD aggregates and domain events
- ❌ Input→Command pattern separation

**Using instead**:

- ✅ Simple POCOs for entities
- ✅ Direct service calls
- ✅ Data annotations for validation
- ✅ Standard request/response models

---

## Technology Stack

### Backend

- .NET Core 10
- Hot Chocolate GraphQL 15.x
- Entity Framework Core 10
- PostgreSQL 16
- JWT Bearer authentication
- Keycloak OAuth 2.0 / OIDC

### Frontend

- Angular 21 (standalone components)
- Apollo Client (GraphQL)
- Tailwind CSS 3.x
- TypeScript 5.x

### Infrastructure

- Docker Compose
- PostgreSQL with Row-Level Security
- Keycloak 23.0.4

---

## OAuth 2.0 Flow (Keycloak)

### Authentication Flow

```
1. User clicks "Sign in" → Frontend redirects to Keycloak
2. User authenticates → Keycloak redirects back with auth code
3. Frontend exchanges code for tokens (PKCE) → Gets JWT
4. Frontend stores JWT in localStorage
5. Frontend sends GraphQL requests with Authorization header
6. Backend validates JWT → Extracts sub claim → Looks up user in database
7. Backend sets RLS variables from database (user.Id, user.FamilyId)
8. PostgreSQL enforces RLS policies → Returns only user's data
```

### JWT Claims (Standard OIDC Only)

**Keycloak provides only standard OIDC claims**:

- `sub` - Keycloak user ID (maps to User.ExternalUserId in database)
- `email` - User's email address
- `name` - User's display name
- `email_verified` - Email verification status (boolean)
- `exp` - Token expiration timestamp
- `iat` - Token issued at timestamp
- `iss` - Issuer (Keycloak realm URL)
- `aud` - Audience (familyhub-web)

**Family context comes from PostgreSQL**:

- `user.FamilyId` - Stored in database, queried via GraphQL
- Roles - Managed in database or Keycloak realm roles (not custom attributes)
- No custom JWT claims needed

---

## Database Multi-Tenancy (RLS)

### PostgreSQL Row-Level Security

**Two schemas**:

- `auth` - Users table
- `family` - Families table

**RLS Policies enforce**:

- Users can only see their own data
- Users can only see data from their family
- Enforced at database level (defense in depth)

**Implementation**:

1. JWT validated → Claims extracted
2. PostgresRlsMiddleware sets session variables
3. All queries automatically filtered by RLS policies

```sql
-- Set session variable from JWT
SELECT set_config('app.current_user_id', '{userId}', false);

-- RLS policy uses session variable
CREATE POLICY user_self_policy ON auth.users
    USING ("Id"::text = current_setting('app.current_user_id', true));
```

---

## Adding New Features

### Step 1: Create Feature Folder

```bash
mkdir -p src/FamilyHub.Api/Features/NewFeature/{Models,Services,GraphQL,Data}
```

### Step 2: Create Entity Model

```csharp
// Features/NewFeature/Models/NewEntity.cs
public class NewEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // ... other properties
}
```

### Step 3: Add to AppDbContext

```csharp
// Common/Database/AppDbContext.cs
public DbSet<NewEntity> NewEntities { get; set; }
```

### Step 4: Create EF Core Configuration

```csharp
// Features/NewFeature/Data/NewEntityConfiguration.cs
public class NewEntityConfiguration : IEntityTypeConfiguration<NewEntity>
{
    public void Configure(EntityTypeBuilder<NewEntity> builder)
    {
        builder.ToTable("new_entities", "newfeature");
        // ... configure columns
    }
}
```

### Step 5: Create Migration

```bash
dotnet ef migrations add AddNewFeature --project src/FamilyHub.Api
dotnet ef database update --project src/FamilyHub.Api
```

### Step 6: Create Service

```csharp
// Features/NewFeature/Services/NewFeatureService.cs
public class NewFeatureService
{
    private readonly AppDbContext _context;

    public async Task<NewEntityDto> CreateAsync(CreateNewEntityRequest request)
    {
        // Business logic
    }
}
```

### Step 7: Create GraphQL

```csharp
// Features/NewFeature/GraphQL/NewFeatureMutations.cs
public class NewFeatureMutations
{
    [Authorize]
    public async Task<NewEntityDto> CreateNewEntity(
        CreateNewEntityRequest input,
        [Service] NewFeatureService service)
    {
        return await service.CreateAsync(input);
    }
}
```

### Step 8: Register in Program.cs

```csharp
builder.Services.AddScoped<NewFeatureService>();

builder.Services
    .AddGraphQLServer()
    // ...
    .AddTypeExtension<NewFeatureMutations>();
```

---

## Migration Path

### When to Extract to Modules/Services

**Phase 2-3** (Optional):

- If a feature exceeds 15 files
- If a feature has distinct deployment needs
- If performance requires service separation

**Phase 5+** (Microservices):

- Extract to separate services with own databases
- Use message broker (RabbitMQ) for inter-service communication
- Deploy to Kubernetes

**Migration Strategy**: Strangler Fig Pattern

1. Create new service
2. Dual-write to both
3. Migrate reads
4. Remove from monolith

---

## Testing Strategy

### Unit Tests

Test services and business logic in isolation using in-memory database:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;

var context = new AppDbContext(options);
var service = new AuthService(context);
```

### Integration Tests

Test full HTTP pipeline using WebApplicationFactory:

```csharp
public class GraphQLApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}
```

---

## Comparison: Original vs. Simplified

| Aspect | Original Plan | Simplified Architecture |
|--------|--------------|------------------------|
| Projects | 7 (API, SharedKernel, 2 modules, 3 test) | 3 (API, 2 test) |
| Organization | DDD layers | Feature folders |
| DbContext | One per module | Single AppDbContext |
| Value Objects | Vogen everywhere | Standard types |
| Commands | MediatR CQRS | Direct service calls |
| Validation | FluentValidation | Data annotations |
| Timeline | 8 weeks | 4 weeks |
| LOC | ~15,000 | ~5,000 |

---

## References

- **ADR-001**: Simple Monolith First (updated)
- **ADR-002**: OAuth with Keycloak (updated)
- **BACKEND_DEVELOPMENT.md**: Updated with feature folder patterns
- **Original Modular Plan**: Preserved in Git commit history

---

**Last Updated**: 2026-02-01
**Author**: Claude Sonnet 4.5
**Status**: Active for Phase 0
