# Standards for Family Creation Post-Login

The following standards apply to this work and guide our implementation decisions.

---

## 1. backend/graphql-input-command

**Source**: `agent-os/standards/backend/graphql-input-command.md`

# GraphQL Input→Command Pattern

Separate Input DTOs (primitives) from MediatR Commands (Vogen). See ADR-003.

## Why

Hot Chocolate cannot deserialize Vogen value objects. This creates clean separation between presentation and domain layers.

## GraphQL Input (primitives only)

```csharp
public sealed record CreateFamilyInput
{
    [Required]
    public required string Name { get; init; }
}
```

## MediatR Command (Vogen types)

```csharp
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;
```

## Mutation (mapping layer)

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

## Rules

- Input DTOs: `Presentation/DTOs/{Name}Input.cs`
- Commands: `Application/Commands/{Name}Command.cs`
- Handlers: `Application/Handlers/{Name}CommandHandler.cs`
- Never use Vogen types in GraphQL input types
- Always validate at Vogen boundary (`.From()` throws if invalid)

---

## 2. frontend/angular-components

**Source**: `agent-os/standards/frontend/angular-components.md`

# Angular Components

All components are standalone (no NgModules). Use atomic design hierarchy.

## Standalone Component

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

## Atomic Design Hierarchy

- **Atoms:** Button, Input, Icon (basic building blocks)
- **Molecules:** FormField, SearchBar (atoms combined)
- **Organisms:** Sidebar, Header, Card (complex components)
- **Templates:** PageLayout (page structure without data)
- **Pages:** DashboardPage, FamilyPage (complete pages with data)

## File Organization

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

## Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state
- Follow atomic design for organization

---

## 3. frontend/apollo-graphql

**Source**: `agent-os/standards/frontend/apollo-graphql.md`

# Apollo GraphQL

Use Apollo Client for GraphQL with typed operations.

## Query Pattern

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

## Mutation Pattern

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

## Error Handling

```typescript
family$ = this.apollo.query({ query: GET_CURRENT_FAMILY }).pipe(
  map(result => result.data.currentFamily),
  catchError(error => {
    console.error('GraphQL Error:', error);
    return of(null);
  })
);
```

## Rules

- Use `inject(Apollo)` for dependency injection
- Handle errors with catchError
- Use typed operations (gql tagged templates)

---

## 4. testing/playwright-e2e

**Source**: `agent-os/standards/testing/playwright-e2e.md`

# Playwright E2E Testing

API-first testing approach. Zero retry policy. Multi-browser support.

## Test Structure

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

## API-First Setup

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

## Page Object Model

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

## Configuration

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

## Rules

- Zero retries - fix flaky tests, don't mask them
- Use data-testid for selectors
- API-first: setup data via GraphQL, not UI
- Multi-browser: test chromium, firefox, webkit
- Location: `e2e/{feature}.spec.ts`

---

## How These Standards Apply to Our Feature

### Backend (Standard #1: graphql-input-command)

**Current Implementation**: ✅ Already follows this standard

- `CreateFamilyInput` uses primitive `string` for name
- `CreateFamilyCommand` uses Vogen `FamilyName` value object
- Mutation resolver maps Input → Command
- **No changes needed** - we're just using what exists

### Frontend Service Layer (Standard #3: apollo-graphql)

**Implementation**:

- Create `family.operations.ts` with GraphQL mutation definition
- `FamilyService` uses `inject(Apollo)` pattern
- `apollo.mutate()` with proper error handling via `catchError()`
- Returns Observable that components can subscribe to

### Frontend Component (Standard #2: angular-components)

**Implementation**:

- `CreateFamilyDialogComponent` is standalone component
- Uses Angular Signals for reactive state (`familyName`, `isLoading`, `errorMessage`)
- Imports: `CommonModule`, `FormsModule`
- Classifies as "Organism" in atomic design hierarchy
- Located in `features/family/components/` following project structure

### Testing (Standard #4: playwright-e2e)

**Implementation**:

- Create `e2e/family/family-creation-post-login.spec.ts`
- Zero retry policy (forces us to write stable tests)
- Use `data-testid` attributes for selectors
- Test multiple scenarios: auto-show, create, dismiss, validation
- Multi-browser support (chromium, firefox, webkit)

---

## Standard Compliance Checklist

When implementing this feature, verify:

- [ ] **Backend**: No changes needed (already compliant)
- [ ] **GraphQL Operations**: Created in separate `.operations.ts` file
- [ ] **Service**: Uses `inject(Apollo)` and proper error handling
- [ ] **Component**: Standalone with `standalone: true`
- [ ] **Component State**: Uses Angular Signals (not legacy observables for local state)
- [ ] **Component Imports**: Properly declared in `imports` array
- [ ] **E2E Tests**: Located in `e2e/` folder
- [ ] **E2E Tests**: Use `data-testid` selectors
- [ ] **E2E Tests**: Zero retries configured
- [ ] **E2E Tests**: Cover main user journeys

---

## Educational Note

**Why standards matter**:

These standards ensure consistency across the codebase, making it easier for:

1. **New developers** to understand patterns quickly
2. **Code reviews** to focus on logic, not style debates
3. **Maintenance** by keeping architecture predictable
4. **Testing** through reliable, stable patterns
5. **Refactoring** with confidence that patterns are followed

By following these standards, we're not just building a feature—we're contributing to a **maintainable, professional codebase**.
