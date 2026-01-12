# Development Workflows

**Purpose:** Detailed patterns for common development tasks. Load this document when implementing features that require specific workflow knowledge.

**When to reference:** Database migrations, value objects, testing, GraphQL integration, E2E tests.

---

## Database Migrations with EF Core

**CRITICAL:** Use EF Core Code-First migrations for ALL schema changes (never custom SQL scripts).

### Pattern

One DbContext per module (Auth, Calendar, etc.), each targeting its own PostgreSQL schema. Fluent API configurations in `IEntityTypeConfiguration<T>` classes, PostgreSQL-specific features (RLS, triggers) via `migrationBuilder.Sql()`.

### Commands

```bash
# Create migration
dotnet ef migrations add <Name> --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# Apply migration (development)
dotnet ef database update --context AuthDbContext

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

### Vogen Integration

```csharp
// In IEntityTypeConfiguration<User>
builder.Property(u => u.Id)
    .HasConversion(new UserId.EfCoreValueConverter())
    .IsRequired();
```

### Reference

Original SQL design scripts in `/database/docs/reference/sql-design/` (informational only, NOT executed).

---

## Value Objects with Vogen

**CRITICAL:** Use Vogen for ALL value objects (never manual base classes).

### Pattern

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    // Vogen source generator auto-generates:
    // - Equality operators
    // - EF Core converter
    // - JSON serialization
    // - Validation
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty");

        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
```

### Creation

```csharp
// New GUID
UserId userId = UserId.New();

// With validation
Email email = Email.From("user@example.com"); // Throws if invalid
Email.TryFrom("invalid", out var result);     // Safe creation

// In tests (manual creation)
var testEmail = Email.From("test@example.com");
```

### EF Core Configuration

```csharp
builder.Property(u => u.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(255);
```

### Examples

See `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/` for Email, UserId, FamilyId patterns.

---

## GraphQL Input/Command Pattern

**CRITICAL:** Maintain separate GraphQL Input DTOs (primitive types) that map to MediatR Commands (Vogen value objects).

### Why

HotChocolate cannot natively deserialize Vogen value objects from JSON. Input → Command mapping provides explicit conversion point and framework compatibility.

### Pattern

```csharp
// GraphQL Input DTO (primitives)
public record CreateFamilyInput
{
    public string Name { get; init; } = string.Empty;
}

// MediatR Command (Vogen value objects)
public record CreateFamilyCommand(FamilyName Name) : IRequest<CreateFamilyPayload>;

// Mutation method
public async Task<CreateFamilyPayload> CreateFamilyAsync(
    CreateFamilyInput input,
    [Service] IMediator mediator)
{
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name) // Explicit conversion
    );

    return await mediator.Send(command);
}
```

### Decision Rationale

[ADR-003](../architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md) - Attempted command-as-input pattern, failed due to Vogen incompatibility.

---

## GraphQL Mapper Pattern

**CRITICAL:** Use convention-based mapper pattern for converting command results to GraphQL payload types (replaces old factory DI pattern).

### Why

The mapper pattern provides:

- **70% reduction in boilerplate** - No factory classes or DI registration
- **Zero code duplication** - Centralized mappers reused across mutations
- **Auto-mapping for simple cases** - Property name matching with intelligent type conversion
- **Manual override when needed** - ToGraphQLType() extensions for complex mappings
- **Better architecture** - Eliminates post-command repository calls in presentation layer

### Two Approaches

#### Auto-Mapping Convention (Default)

MutationHandler automatically maps command result properties to payload constructor parameters:

**Features:**

- Case-insensitive property name matching
- Automatic Vogen `.Value` unwrapping
- Automatic enum helper detection (e.g., `Role.AsRoleType()`)
- Supports parameterless, single-param, and multi-param (tuple) constructors

**When to use:**

- Simple property mappings
- Basic type conversions (Guid, string, DateTime, bool, int)
- Vogen value objects that need unwrapping
- Enum conversions with `.AsXxxType()` helper methods

**Example (no code needed):**

```csharp
// Command result
public record AcceptInvitationResult(FamilyId FamilyId, FamilyName FamilyName, UserRole Role);

// Payload constructor (auto-mapping matches properties by name)
public AcceptInvitationPayload(Guid familyId, string familyName, UserRoleType role)
{
    FamilyId = familyId;
    FamilyName = familyName;
    Role = role;
}

// No ToGraphQLType() needed! MutationHandler:
// 1. Matches FamilyId → familyId (case-insensitive)
// 2. Unwraps FamilyId.Value (Vogen)
// 3. Auto-detects Role.AsRoleType() extension for enum conversion
```

**Error Handling:**

If auto-mapping fails, you get a descriptive `AutoMappingException`:

```
Failed to auto-map AcceptInvitationResult to AcceptInvitationPayload:
Property 'InvalidField' not found in result type.
Consider adding a ToGraphQLType() extension method.
```

#### Manual Override (Complex Cases)

Add a `ToGraphQLType()` extension method when auto-mapping can't handle:

**When to use:**

- Nested object creation
- Calculated fields
- Multiple enum conversions with different helpers
- Complex transformations

**Example:**

```csharp
// Extension method in AuthResultExtensions.cs
public static class AuthResultExtensions
{
    // Complex nested object - requires manual mapping
    public static AuthenticationResult ToGraphQLType(this CompleteZitadelLoginResult result)
    {
        return new AuthenticationResult
        {
            User = new UserType  // Nested object creation
            {
                Id = result.UserId.Value,
                Email = result.Email.Value,
                EmailVerified = result.EmailVerified,
                FamilyId = result.FamilyId.Value,
                AuditInfo = result.AsAuditInfo()  // Custom mapping
            },
            AccessToken = result.AccessToken,
            RefreshToken = null,
            ExpiresAt = result.ExpiresAt
        };
    }

    // Calculated field - requires manual mapping
    public static CreatedFamilyDto ToGraphQLType(this CreateFamilyResult result)
    {
        return new CreatedFamilyDto
        {
            Id = result.FamilyId.Value,
            Name = result.Name.Value,
            OwnerId = result.OwnerId.Value,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.CreatedAt  // Calculated: same as CreatedAt for new families
        };
    }
}
```

**Precedence:** If both exist, manual ToGraphQLType() takes precedence over auto-mapping.

### Pattern

```csharp
// 1. MAPPERS - Centralized mapping logic (Presentation/GraphQL/Mappers/)
public static class UserMapper
{
    // Maps domain entity to GraphQL type
    public static UserType AsGraphQLType(User user)
    {
        return new UserType
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            EmailVerified = user.EmailVerified,
            FamilyId = user.FamilyId.Value,
            AuditInfo = MapperBase.AsAuditInfo(user.CreatedAt, user.UpdatedAt)
        };
    }
}

public static class InvitationMapper
{
    // Maps value object enums to GraphQL enums
    public static UserRoleType AsRoleType(UserRole role)
    {
        return role.Value.ToLowerInvariant() switch
        {
            "owner" => UserRoleType.OWNER,
            "admin" => UserRoleType.ADMIN,
            "member" => UserRoleType.MEMBER,
            _ => throw new InvalidOperationException($"Unknown role: {role.Value}")
        };
    }
}

// 2. EXTENSIONS - ToGraphQLType() for each command result (Presentation/GraphQL/Extensions/)
public static class AuthResultExtensions
{
    // Single object return (most common)
    public static AuthenticationResult ToGraphQLType(this CompleteZitadelLoginResult result)
    {
        return new AuthenticationResult
        {
            User = UserMapper.AsUserType(
                result.UserId,
                result.Email,
                result.EmailVerified,
                result.FamilyId,
                result.CreatedAt,
                result.UpdatedAt),
            AccessToken = result.AccessToken,
            ExpiresAt = result.ExpiresAt
        };
    }

    // Tuple return (multiple constructor parameters)
    public static (Guid FamilyId, string FamilyName, UserRoleType Role) ToGraphQLType(
        this AcceptInvitationResult result)
    {
        return (
            result.FamilyId.Value,
            result.FamilyName.Value,
            InvitationMapper.AsRoleType(result.Role)
        );
    }

    // Null return (parameterless constructor)
    public static object? ToGraphQLType(this Result result)
    {
        return null; // Signals MutationHandler to use parameterless constructor
    }
}

// 3. USAGE - MutationHandler automatically discovers and invokes extensions
public async Task<CompleteZitadelLoginPayload> CompleteZitadelLoginAsync(
    CompleteZitadelLoginInput input,
    [Service] IMutationHandler mutationHandler,
    [Service] IMediator mediator)
{
    return await mutationHandler.Handle<CompleteZitadelLoginResult, CompleteZitadelLoginPayload>(
        async () =>
        {
            var command = new CompleteZitadelLoginCommand(
                AuthorizationCode.From(input.Code),
                ZitadelCallbackUri.From(input.RedirectUri));

            var result = await mediator.Send(command);
            return result; // MutationHandler calls result.ToGraphQLType() via reflection
        });
}
```

### Three ToGraphQLType() Patterns

The MutationHandler supports three constructor patterns:

#### 1. Single Object Return (Most Common)

```csharp
// Extension returns single object
public static CreatedFamilyDto ToGraphQLType(this CreateFamilyResult result)
{
    return new CreatedFamilyDto
    {
        Id = result.FamilyId.Value,
        Name = result.Name.Value,
        OwnerId = result.OwnerId.Value,
        CreatedAt = result.CreatedAt,
        UpdatedAt = result.CreatedAt
    };
}

// Payload constructor (single parameter)
public CreateFamilyPayload(CreatedFamilyDto family)
{
    Family = family;
}
```

#### 2. Tuple Return (Multiple Parameters)

```csharp
// Extension returns tuple
public static (Guid InvitationId, UserRoleType Role) ToGraphQLType(
    this UpdateInvitationRoleResult result)
{
    return (
        result.InvitationId.Value,
        InvitationMapper.AsRoleType(result.Role)
    );
}

// Payload constructor (multiple parameters matching tuple)
public UpdateInvitationRolePayload(Guid invitationId, UserRoleType role)
{
    InvitationId = invitationId;
    Role = role;
}
```

#### 3. Null Return (Parameterless Constructor)

```csharp
// Extension returns null
public static object? ToGraphQLType(this Result result)
{
    return null; // Signals parameterless constructor
}

// Payload constructor (parameterless)
public CancelInvitationPayload()
{
    IsSuccess = true;
}
```

### Auto-Mapping Algorithm

MutationHandler follows this decision tree:

1. **Check for manual ToGraphQLType()** - If found, use it (manual override takes precedence)
2. **Analyze payload constructor:**
   - **Parameterless** → Return null (signals parameterless constructor)
   - **Single parameter** → Find matching result property, extract value
   - **Multiple parameters** → Build tuple from matched properties
3. **Property matching:** Case-insensitive name match (e.g., `FamilyId` → `familyId`)
4. **Value extraction:**
   - Direct match (primitives: Guid, string, DateTime, bool, int)
   - Vogen unwrapping (detect `.Value` property)
   - Enum helper detection (auto-find `.AsXxxType()` extension methods)
5. **Error handling:** Throw `AutoMappingException` with descriptive message if any step fails

**Performance:** Reflection results cached in `ConcurrentDictionary` (<5ms overhead per mutation)

**Limitations:**

- Supports up to 5 constructor parameters (C# tuple limitation)
- Cannot auto-map nested object creation
- Cannot handle calculated fields or complex transformations

### Directory Structure

```
Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/
├── Mappers/
│   ├── UserMapper.cs          # Domain entity → GraphQL type
│   ├── InvitationMapper.cs    # Enum mappings, shared logic
│   └── FamilyMapper.cs        # Family-specific mappings
├── Extensions/
│   └── AuthResultExtensions.cs # ToGraphQLType() for all Auth command results
├── Payloads/
│   ├── CompleteZitadelLoginPayload.cs
│   ├── CreateFamilyPayload.cs
│   └── AcceptInvitationPayload.cs
└── Mutations/
    └── AuthMutations.cs       # GraphQL mutation methods
```

### Naming Conventions

- **Mappers**: `AsGraphQLType()` or `As{TargetType}()` (e.g., `AsRoleType()`)
- **Extensions**: `ToGraphQLType()` (MUST be this exact name for source generator)
- **Location**: `{Module}.Presentation.GraphQL.Mappers` and `.Extensions` namespaces

### Error Handling

MutationHandler ONLY handles errors - mappers handle success cases:

```csharp
// GOOD - Mapper only handles success case
public static UserType AsGraphQLType(User user)
{
    return new UserType { /* ... */ };
}

// BAD - Don't handle errors in mappers
public static UserType? AsGraphQLType(User? user)
{
    if (user == null)
        return null; // MutationHandler already handles this
    // ...
}
```

### Migration from Old Pattern

Old pattern (deprecated):

```csharp
// Factory class with DI (DELETED)
public class CreateFamilyPayloadFactory(IFamilyRepository repository)
    : IPayloadFactory<CreateFamilyResult, CreateFamilyPayload>
{
    public CreateFamilyPayload Success(CreateFamilyResult result)
    {
        // Anti-pattern: Repository call in presentation layer
        var family = repository.GetByIdAsync(result.FamilyId).GetAwaiter().GetResult();
        return new CreateFamilyPayload(family);
    }
}

// DI registration (DELETED)
services.AddScoped<IPayloadFactory<CreateFamilyResult, CreateFamilyPayload>,
    CreateFamilyPayloadFactory>();
```

New pattern:

```csharp
// Static extension (NO DI)
public static CreatedFamilyDto ToGraphQLType(this CreateFamilyResult result)
{
    return new CreatedFamilyDto
    {
        Id = result.FamilyId.Value,
        Name = result.Name.Value,
        OwnerId = result.OwnerId.Value,
        CreatedAt = result.CreatedAt,
        UpdatedAt = result.CreatedAt // Use data already in result
    };
}

// NO DI registration needed - convention-based discovery
```

### Common Utilities

`MapperBase` provides shared mapping logic:

```csharp
public static class MapperBase
{
    public static AuditInfoType AsAuditInfo(DateTime createdAt, DateTime updatedAt)
    {
        return new AuditInfoType
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
```

Location: `FamilyHub.Infrastructure/GraphQL/MapperBase.cs`

### Testing

Mappers are pure functions - easy to unit test:

```csharp
[Fact]
public void AsGraphQLType_ValidUser_MapsCorrectly()
{
    // Arrange
    var user = new User
    {
        Id = UserId.New(),
        Email = Email.From("test@example.com"),
        EmailVerified = true,
        FamilyId = FamilyId.New(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // Act
    var result = UserMapper.AsGraphQLType(user);

    // Assert
    result.Id.Should().Be(user.Id.Value);
    result.Email.Should().Be(user.Email.Value);
    result.EmailVerified.Should().BeTrue();
}
```

### Reference

- MutationHandler: `src/api/FamilyHub.SharedKernel/Presentation/GraphQL/MutationHandler.cs`
- AutoMappingException: `src/api/FamilyHub.SharedKernel/Presentation/GraphQL/AutoMappingException.cs`
- Example implementation: `src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/`
- Auth migrations: 3/6 Auth mutations use auto-mapping, 3/6 use manual override

---

## Testing Patterns

### FluentAssertions

**CRITICAL:** Use FluentAssertions for ALL assertions (never xUnit `Assert.*`).

```csharp
// Basic assertions
actual.Should().Be(expected);
result.Should().NotBeNull();
collection.Should().HaveCount(3);

// Async assertions
await act.Should().ThrowAsync<InvalidOperationException>();
await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));

// Object assertions
user.Should().BeEquivalentTo(expected, options => options
    .Excluding(u => u.Id)
    .Excluding(u => u.CreatedAt));
```

Docs: <https://fluentassertions.com/>

### AutoFixture with NSubstitute

**CRITICAL:** Use `[Theory, AutoNSubstituteData]` for ALL tests with dependencies.

```csharp
[Theory, AutoNSubstituteData]
public async Task CreateFamily_Success(
    // Dependencies auto-injected by AutoFixture
    IFamilyRepository repository,
    IMediator mediator,
    CreateFamilyCommand command)
{
    // Arrange - configure only what matters
    repository.ExistsByNameAsync(Arg.Any<FamilyName>())
        .Returns(false);

    // Act
    var handler = new CreateFamilyCommandHandler(repository, mediator);
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    await repository.Received(1).AddAsync(Arg.Any<Family>());
}
```

**Vogen Policy:** Always create Vogen value objects manually in tests (improves clarity):

```csharp
// GOOD
var familyName = FamilyName.From("Test Family");
var userId = UserId.New();

// BAD (don't let AutoFixture generate Vogen types)
// AutoFixture can't properly generate valid Vogen instances
```

Custom attribute location: `/src/api/tests/FamilyHub.Tests.Unit/AutoNSubstituteDataAttribute.cs`

---

## E2E Testing with Playwright

**CRITICAL:** Use Playwright for ALL E2E tests (migrated from Cypress January 2026).

### Test Structure

```
e2e/
├── fixtures/          # Reusable test fixtures (auth, graphql, rabbitmq)
├── support/           # Helper utilities (constants, vogen-mirrors, api-helpers)
├── tests/             # Test files (.spec.ts)
├── global-setup.ts    # Testcontainers lifecycle
└── global-teardown.ts # Cleanup
```

### Key Patterns

#### 1. Fixtures (Dependency Injection)

```typescript
test("should create family", async ({
  authenticatedPage,
  interceptGraphQL,
}) => {
  await interceptGraphQL("GetCurrentFamily", { data: { family: null } });
  await authenticatedPage.goto("/family/create");
  // Test uses OAuth tokens automatically
});
```

#### 2. Vogen TypeScript Mirrors

```typescript
// Mirror C# Vogen validation in TypeScript
const familyName = FamilyName.from("Smith Family"); // Throws if invalid
const userId = UserId.new(); // Generates new GUID
```

#### 3. API-First Event Chain Testing

```typescript
test("doctor appointment event chain", async ({ rabbitmq }) => {
  // 1. Create via GraphQL API (10x faster than UI)
  const result = await client.mutate(CREATE_APPOINTMENT_MUTATION, variables);

  // 2. Verify RabbitMQ event published
  const event = await rabbitmq.waitForMessage(
    (msg) => msg.eventType === "HealthAppointmentScheduled",
    5000
  );

  // 3. Query backend to verify entities created
  const calendarEvents = await client.query(GET_CALENDAR_EVENTS);

  // 4. Spot-check UI (optional)
  await page.goto("/calendar");
  await expect(page.getByText("Doctor: Dr. Smith")).toBeVisible();
});
```

### Running Tests

```bash
# Local development
npm run e2e              # UI mode (interactive debugging)
npm run e2e:headless     # Headless mode
npm run e2e:chromium     # Single browser
npm run e2e:debug        # Debug mode with breakpoints

# CI/CD
npx playwright test      # Runs all tests on 3 browsers
```

### Test Organization

- **family-creation.spec.ts**: Main E2E tests (happy path, validation, errors)
- **accessibility.spec.ts**: WCAG 2.1 AA compliance (axe-core)
- **cross-browser.spec.ts**: Smoke tests (Chromium, Firefox, WebKit)
- **event-chains.spec.ts**: Event chain templates (SKIPPED until Phase 2)

### Zero-Retry Policy

`retries: 0` forces fixing flaky tests immediately (never mask issues with retries).

### Reference

[ADR-004-PLAYWRIGHT-MIGRATION.md](../architecture/ADR-004-PLAYWRIGHT-MIGRATION.md) - Migration rationale, patterns, metrics.

---

## Git Workflow

### Commit Format

```
<type>(<scope>): <summary> (#<issue>)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

**Types:** feat, fix, docs, style, refactor, test, chore

**Examples:**

```
feat(auth): add OAuth 2.0 flow (#42)
fix(calendar): resolve timezone offset bug (#58)
test(family): add creation validation tests (#61)
```

### Branching

See [git-workflow.md](git-workflow.md) for branching strategy and automation.

---

## Automatic Code Formatting

### Hook Configuration

Family Hub uses Claude Code PostToolUse hooks to automatically format code after AI-assisted edits.

**Configured formatters:**

- **TypeScript/JavaScript:** Prettier + ESLint (frontend)
- **C# files:** dotnet format (backend)
- **Markdown files:** markdownlint-cli2 (documentation) ← NEW
- **JSON/YAML files:** Prettier (configuration) ← NEW

**Configuration:** `.claude/settings.json` (committed to git)

**How it works:**

1. Claude edits a file using Edit or Write tool
2. Hook detects file extension
3. Appropriate formatter runs automatically
4. Changes appear in next file read/git diff

**Manual formatting:**

Frontend:

```bash
cd src/frontend/family-hub-web
npm run lint -- --fix
npm run lint:md:fix  # Markdown
npx prettier --write "src/**/*.{ts,js,html,css,scss}"
```

Backend:

```bash
cd src/api
dotnet format
```

Documentation:

```bash
npx markdownlint-cli2 --fix "**/*.md"  # All markdown files
npx prettier --write "**/*.{json,yml,yaml}"  # All config files
```

**Token Savings:**

- Markdownlint: **40% reduction** for documentation-heavy sessions (191 markdown files, 280K words)
- JSON/YAML formatting: Consistent configuration files reduce context noise

**Troubleshooting:**

If hooks fail (rare):

1. Check hook execution: Look for errors in Claude Code output
2. Disable temporarily: Add to `.claude/settings.local.json`:

   ```json
   {
     "hooks": {
       "PostToolUse": []
     }
   }
   ```

3. Re-enable: Remove override from `.local.json`

**Full guide:** [HOOKS.md](HOOKS.md) - Comprehensive hook documentation

---

## Performance Testing with k6

**CRITICAL:** Run performance tests before production deployments to verify API response time targets.

### Overview

Family Hub uses [k6](https://k6.io/) for load and stress testing the GraphQL API. Performance tests validate response time thresholds defined in [Section 12.7](../architecture/MODULAR-DOTNET-HOTCHOCOLATE-GUIDE.md).

### Quick Start

```bash
# Install k6 (macOS)
brew install k6

# Install k6 (Linux/Debian)
sudo apt-get install k6

# Run baseline test
cd tests/performance
k6 run scenarios/baseline.js

# Run load test
k6 run scenarios/load.js

# Run stress test
k6 run scenarios/stress.js
```

### Performance Targets

| Metric | Baseline | Load | Stress |
|--------|----------|------|--------|
| p50 | < 50ms | < 200ms | < 500ms |
| p95 | < 150ms | < 500ms | < 1000ms |
| p99 | < 300ms | < 1000ms | < 3000ms |
| Error Rate | < 0.1% | < 1% | < 5% |

### Test Scenarios

| Scenario | VUs | Duration | Purpose |
|----------|-----|----------|---------|
| Baseline | 10 constant | 1 min | Quick validation, CI smoke test |
| Load | 0→50→100→0 | 10 min | Capacity validation, find bottlenecks |
| Stress | 10→200→10 | 3 min | Find breaking point, verify recovery |

### CI/CD Integration

Performance tests run via GitHub Actions:

- **Manual trigger:** Actions → "Performance Tests (k6)" → Run workflow
- **Nightly schedule:** Automatically at 2 AM UTC
- **Scenarios:** baseline, load, stress, or all

**Workflow:** `.github/workflows/performance.yml`

### Environment Configuration

```bash
# Local development (default)
k6 run scenarios/baseline.js

# Specify environment
k6 run -e K6_ENV=ci scenarios/load.js

# Custom GraphQL URL
k6 run -e GRAPHQL_URL=http://custom:5002/graphql scenarios/baseline.js
```

### Directory Structure

```
tests/performance/
├── config/
│   ├── thresholds.js      # Threshold configurations
│   └── environments.js    # Environment settings
├── helpers/
│   └── graphql.js         # GraphQL request helpers
├── scenarios/
│   ├── baseline.js        # Baseline test
│   ├── load.js            # Load test
│   └── stress.js          # Stress test
└── results/               # Test output (git-ignored)
```

### Full Documentation

See [tests/performance/README.md](../../tests/performance/README.md) for complete k6 documentation including:

- Installation instructions (all platforms)
- Detailed test scenario descriptions
- Writing new tests
- Troubleshooting guide

### Related

- **Issue:** #63 - Create k6 Performance Benchmarking Suite
- **Architecture:** [Section 12.7 - Performance Testing](../architecture/MODULAR-DOTNET-HOTCHOCOLATE-GUIDE.md)

---

**Last updated:** 2026-01-12
**Version:** 2.1.0
