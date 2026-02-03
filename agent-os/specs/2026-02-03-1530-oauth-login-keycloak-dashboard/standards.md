# Standards for OAuth Login with Keycloak → Dashboard

The following standards apply to this work.

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from MediatR Commands (Vogen). See ADR-003.

### Why

Hot Chocolate cannot deserialize Vogen value objects. This creates clean separation between presentation and domain layers.

### GraphQL Input (primitives only)

```csharp
public sealed record CreateFamilyInput
{
    [Required]
    public required string Name { get; init; }
}
```

### MediatR Command (Vogen types)

```csharp
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;
```

### Mutation (mapping layer)

```csharp
public async Task<CreateFamilyPayload> CreateFamily(
    CreateFamilyInput input,
    [Service] IMediator mediator)
{
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name)  // Primitive → Vogen
    );
    var result = await mediator.Send(command);
    return new CreateFamilyPayload(result);
}
```

### Rules

- Input DTOs: `Presentation/DTOs/{Name}Input.cs`
- Commands: `Application/Commands/{Name}Command.cs`
- Handlers: `Application/Handlers/{Name}CommandHandler.cs`
- Never use Vogen types in GraphQL input types
- Always validate at Vogen boundary (`.From()` throws if invalid)

---

## backend/vogen-value-objects

Always use Vogen 8.0+ for domain value objects. Never use primitives in commands/domain.

### Definition Pattern

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty.");
        if (value.Length > 320)
            return Validation.Invalid("Email cannot exceed 320 characters.");
        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format.");
        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
```

### Creation

```csharp
UserId userId = UserId.New();           // New GUID
Email email = Email.From("user@ex.com"); // With validation (throws if invalid)
Email.TryFrom("invalid", out var result); // Safe creation
```

### EF Core Configuration

```csharp
builder.Property(u => u.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(320)
    .IsRequired();
```

### Rules

- Always include `conversions: Conversions.EfCoreValueConverter`
- Implement `Validate()` for business rules
- Implement `NormalizeInput()` for string normalization
- Location: `Domain/ValueObjects/{Name}.cs`

---

## frontend/angular-components

All components are standalone (no NgModules). Use atomic design hierarchy.

### Standalone Component

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,  // Required!
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  isCollapsed = signal(false);

  toggleSidebar() {
    this.isCollapsed.update(value => !value);
  }
}
```

### Atomic Design Hierarchy

- **Atoms:** Button, Input, Icon (basic building blocks)
- **Molecules:** FormField, SearchBar (atoms combined)
- **Organisms:** Sidebar, Header, Card (complex components)
- **Templates:** PageLayout (page structure without data)
- **Pages:** DashboardPage, FamilyPage (complete pages with data)

### File Organization

```
app/
├── components/
│   ├── atoms/
│   │   └── button/
│   ├── molecules/
│   │   └── form-field/
│   └── organisms/
│       └── sidebar/
└── pages/
    ├── dashboard/
    └── family/
```

### Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization

---

## frontend/apollo-graphql

Use Apollo Client for GraphQL with typed operations.

### Query Pattern

```typescript
import { gql, Apollo } from 'apollo-angular';

const GET_CURRENT_FAMILY = gql`
  query GetCurrentFamily {
    currentFamily {
      id
      name
      members { id email role }
    }
  }
`;

@Component({ ... })
export class FamilyComponent {
  private apollo = inject(Apollo);

  family$ = this.apollo.query({
    query: GET_CURRENT_FAMILY
  }).pipe(
    map(result => result.data.currentFamily)
  );
}
```

### Mutation Pattern

```typescript
const CREATE_FAMILY = gql`
  mutation CreateFamily($input: CreateFamilyInput!) {
    createFamily(input: $input) {
      familyId
      name
    }
  }
`;

createFamily(name: string) {
  this.apollo.mutate({
    mutation: CREATE_FAMILY,
    variables: { input: { name } }
  }).subscribe({
    next: (result) => console.log('Created:', result.data),
    error: (error) => console.error('Error:', error)
  });
}
```

### Error Handling

```typescript
family$ = this.apollo.query({ query: GET_CURRENT_FAMILY }).pipe(
  map(result => result.data.currentFamily),
  catchError(error => {
    console.error('GraphQL Error:', error);
    return of(null);
  })
);
```

### Rules

- Use `inject(Apollo)` for dependency injection
- Handle errors with catchError
- Use typed operations (gql tagged templates)

---

## database/ef-core-migrations

One DbContext per module, each targeting its own PostgreSQL schema.

### Create Migration

```bash
dotnet ef migrations add MigrationName \
  --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

### Apply Migration

```bash
# Development
dotnet ef database update --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

### Schema Separation

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.HasDefaultSchema("auth");  // Each module has its own schema
}
```

### PostgreSQL RLS

```csharp
// In migration Up() method
migrationBuilder.Sql(@"
    ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

    CREATE POLICY user_isolation_policy ON auth.users
        USING (id = current_setting('app.current_user_id')::uuid);
");
```

### Rules

- Migration name format: `{Timestamp}_{Description}`
- Always test down migrations
- One DbContext per module
- Schema name = module name (lowercase)
- Enable RLS on tenant-isolated tables

---

## testing/playwright-e2e

API-first testing approach. Zero retry policy. Multi-browser support.

### Test Structure

```typescript
import { test, expect } from '@playwright/test';

test.describe('Family Management', () => {
  test('creates a new family', async ({ page }) => {
    await page.goto('/families/create');
    await page.fill('[data-testid="family-name"]', 'Smith Family');
    await page.click('[data-testid="submit-button"]');

    await expect(page).toHaveURL(/\/families\/[a-z0-9-]+/);
    await expect(page.locator('h1')).toContainText('Smith Family');
  });
});
```

### API-First Setup

```typescript
test.beforeEach(async ({ request }) => {
  // Create test data via API
  await request.post('/api/graphql', {
    data: {
      query: `mutation { createTestFamily(name: "Test") { id } }`
    }
  });
});
```

### Page Object Model

```typescript
// pages/family.page.ts
export class FamilyPage {
  constructor(private page: Page) {}

  async createFamily(name: string) {
    await this.page.fill('[data-testid="family-name"]', name);
    await this.page.click('[data-testid="submit"]');
  }

  async expectFamilyCreated(name: string) {
    await expect(this.page.locator('h1')).toContainText(name);
  }
}
```

### Configuration

```typescript
// playwright.config.ts
export default defineConfig({
  retries: 0,  // Zero retry policy!
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
  ],
});
```

### Rules

- Zero retries - fix flaky tests, don't mask them
- Use data-testid for selectors
- API-first: setup data via GraphQL, not UI
- Multi-browser: test chromium, firefox, webkit
- Location: `e2e/{feature}.spec.ts`
