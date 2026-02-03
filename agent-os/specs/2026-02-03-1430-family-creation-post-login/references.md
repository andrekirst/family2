# References for Family Creation Post-Login

This document captures the code references studied during the exploration phase to inform our implementation.

---

## Similar Implementations

### 1. Authentication Flow

**Location**: `src/frontend/family-hub-web/src/app/features/auth/`

**Relevance**: Understanding the complete OAuth 2.0 + PKCE flow with Keycloak to know where post-login logic can be inserted.

**Key Files**:

- `callback/callback.component.ts` - OAuth callback handler
- `login/login.component.ts` - Login initiation
- `graphql/auth.operations.ts` - GraphQL operations (RegisterUser, GetCurrentUser)

**Key Patterns Learned**:

- OAuth flow: Login → Keycloak → Callback → RegisterUser → Dashboard
- `CallbackComponent` calls `UserService.registerUser()` to sync OAuth user with backend
- After sync, navigates to `/dashboard`
- `RegisterUser` mutation returns user data but doesn't include family (family is fetched separately)

**Code Snippet** (callback.component.ts):

```typescript
this.authService.handleCallback().pipe(
  switchMap(() => this.userService.registerUser()),
  tap(() => {
    this.router.navigate(['/dashboard']);
  })
)
```

**Takeaway**: Post-login family check should happen in `DashboardComponent.ngOnInit()`, not in callback. This ensures dashboard loads first, then we optionally show the dialog.

---

### 2. Dashboard Component (Current Implementation)

**Location**: `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.ts`

**Relevance**: This is where we'll integrate the family creation dialog. The dashboard already checks if user has a family and displays different UI accordingly.

**Current Implementation**:

```typescript
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {
  private userService = inject(UserService);
  currentUser = this.userService.currentUser;
}
```

**Template** (dashboard.component.html):

```html
@if (currentUser()!.family) {
  <div class="mt-6 p-4 bg-blue-50 rounded-lg">
    <h3>Family: {{ currentUser()!.family!.name }}</h3>
    <!-- ... -->
  </div>
} @else {
  <div class="mt-6 p-4 bg-gray-50 rounded-lg">
    <h3>No Family Yet</h3>
    <button class="mt-3 px-4 py-2 bg-primary text-white rounded">
      Create Family
    </button>
  </div>
}
```

**Key Patterns Learned**:

- Uses Angular Signals via `currentUser = this.userService.currentUser`
- Uses new Angular control flow (`@if`, not `*ngIf`)
- Tailwind-inspired utility classes for styling
- Button exists but has no click handler

**What We'll Add**:

- `showCreateFamilyDialog = signal(false)` to control dialog visibility
- `ngOnInit()` to check if user has family and show dialog
- `openCreateFamilyDialog()` method to wire up button
- Event handlers for dialog close/creation
- Import and render dialog component

---

### 3. User Service (Backend State Management)

**Location**: `src/frontend/family-hub-web/src/app/core/user/user.service.ts`

**Relevance**: Manages user state and provides methods to register users and fetch current user data. We'll use `fetchCurrentUser()` to refresh after family creation.

**Key Methods**:

```typescript
@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apollo = inject(Apollo);

  currentUser = signal<CurrentUser | null>(null);

  registerUser() {
    return this.apollo.mutate<{ registerUser: CurrentUser }>({
      mutation: REGISTER_USER
    }).pipe(
      map(result => {
        const user = result.data?.registerUser;
        if (user) {
          this.currentUser.set(user);
        }
        return user;
      })
    );
  }

  fetchCurrentUser() {
    return this.apollo.query<{ currentUser: CurrentUser }>({
      query: GET_CURRENT_USER,
      fetchPolicy: 'network-only'
    }).pipe(
      map(result => {
        this.currentUser.set(result.data.currentUser);
        return result.data.currentUser;
      })
    );
  }
}
```

**CurrentUser Interface**:

```typescript
export interface CurrentUser {
  id: string;
  email: string;
  name: string;
  emailVerified: boolean;
  isActive: boolean;
  family?: {
    id: string;
    name: string;
  } | null;
}
```

**Key Patterns Learned**:

- Uses `inject(Apollo)` for dependency injection (modern Angular pattern)
- Signal-based state: `currentUser = signal<CurrentUser | null>(null)`
- `fetchPolicy: 'network-only'` forces refetch (no cache)
- Methods return Observables for component subscription

**What We'll Use**:

- `fetchCurrentUser()` after family creation to refresh user data
- Pattern will guide our `FamilyService` implementation

---

### 4. Auth GraphQL Operations

**Location**: `src/frontend/family-hub-web/src/app/features/auth/graphql/auth.operations.ts`

**Relevance**: Shows how GraphQL operations are structured in this codebase. We'll follow the same pattern for family operations.

**Current Implementation**:

```typescript
import { gql } from 'apollo-angular';

export const REGISTER_USER = gql`
  mutation RegisterUser {
    registerUser {
      id
      email
      name
      emailVerified
      isActive
    }
  }
`;

export const GET_CURRENT_USER = gql`
  query GetCurrentUser {
    currentUser {
      id
      email
      name
      emailVerified
      isActive
      family {
        id
        name
      }
    }
  }
`;
```

**Key Patterns Learned**:

- GraphQL operations exported as constants
- Uses `gql` tagged template literals
- Query/mutation names match backend schema
- Returns only needed fields (no over-fetching)

**What We'll Create**:

```typescript
export const CREATE_FAMILY = gql`
  mutation CreateFamily($input: CreateFamilyInput!) {
    createFamily(input: $input) {
      id
      name
      ownerId
      createdAt
      memberCount
    }
  }
`;
```

---

### 5. Family Domain (Backend)

**Location**: `src/FamilyHub.Api/Features/Family/`

**Relevance**: Understanding the backend domain model and GraphQL API that already exists. This confirms we don't need any backend changes.

#### Family Entity

**File**: `Domain/Entities/Family.cs`

```csharp
public class Family
{
    public FamilyId Id { get; private set; }
    public FamilyName Name { get; private set; }
    public UserId OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public ICollection<User> Members { get; private set; }
}
```

**Takeaway**: Family is an aggregate root with Vogen value objects (FamilyId, FamilyName). Owner is the user who created it.

#### CreateFamily Mutation

**File**: `GraphQL/FamilyMutations.cs`

```csharp
[Authorize]
public async Task<FamilyDto> CreateFamily(
    CreateFamilyInput input,
    [Service] IMediator mediator)
{
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name)
    );

    var result = await mediator.Send(command);
    return new FamilyDto
    {
        Id = result.Family.Id.Value,
        Name = result.Family.Name.Value,
        OwnerId = result.Family.OwnerId.Value,
        CreatedAt = result.Family.CreatedAt,
        MemberCount = result.Family.Members.Count
    };
}
```

**Key Patterns Learned**:

- `[Authorize]` attribute ensures only authenticated users can create families
- Input→Command pattern: `CreateFamilyInput` (primitive) → `CreateFamilyCommand` (Vogen)
- `FamilyName.From(input.Name)` validates and creates value object (throws if invalid)
- Uses MediatR for command handling
- Returns DTO, not domain entity

**Input Model**:

```csharp
public class CreateFamilyInput
{
    [Required]
    public required string Name { get; init; }
}
```

**Takeaway**: Backend is complete! We just need to call this mutation from the frontend.

---

### 6. User-Family Relationship (Backend)

**Location**: `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs`

**Relevance**: Understanding how users are linked to families.

**User Entity**:

```csharp
public class User
{
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public string Name { get; private set; }
    public ExternalUserId ExternalUserId { get; private set; }
    public bool EmailVerified { get; private set; }
    public bool IsActive { get; private set; }

    // Family relationship
    public FamilyId? FamilyId { get; private set; }  // ← NULLABLE!
    public Family? Family { get; private set; }
}
```

**Key Insight**: `FamilyId` is **nullable**, meaning users can exist without a family. This is exactly what we need for the post-login check.

---

### 7. E2E Testing Pattern

**Location**: `e2e/auth/oauth-complete-flow.spec.ts`

**Relevance**: Understanding how authentication E2E tests are structured. We'll follow the same pattern for family creation tests.

**Current Test Structure**:

```typescript
import { test, expect } from '@playwright/test';

test.describe('OAuth Complete Flow', () => {
  test('completes full OAuth login flow', async ({ page }) => {
    // Step 1: Navigate to app
    await page.goto('/');

    // Step 2: Click login button
    await page.click('[data-testid="login-button"]');

    // Step 3: Handle OAuth redirect (Keycloak)
    await page.waitForURL(/keycloak/);
    await page.fill('[name="username"]', process.env.TEST_USER_EMAIL!);
    await page.fill('[name="password"]', process.env.TEST_USER_PASSWORD!);
    await page.click('[type="submit"]');

    // Step 4: Verify callback and redirect to dashboard
    await page.waitForURL('/dashboard');

    // Step 5: Verify user is logged in
    await expect(page.locator('text=Welcome')).toBeVisible();
  });
});
```

**Key Patterns Learned**:

- Uses `data-testid` attributes for reliable selectors
- `waitForURL()` for navigation assertions
- Environment variables for test credentials
- Tests complete user journey (not just individual steps)

**What We'll Create**:

- Similar test structure for family creation flow
- Test scenarios: auto-show dialog, create family, dismiss dialog, validation
- Use existing OAuth test setup (if available)

---

## Documentation References

### Architecture Decision Records (ADRs)

#### ADR-002: OAuth with Keycloak

**Location**: `docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md`

**Relevance**: Complete understanding of OAuth 2.0 + PKCE flow.

**Key Points**:

- Authorization Code Flow with PKCE (RFC 7636)
- Frontend generates code verifier and challenge (S256)
- Backend extracts user data from JWT claims (never trusts client input)
- User sync happens via `RegisterUser` mutation

#### ADR-003: GraphQL Input→Command Pattern

**Location**: `docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md`

**Relevance**: Explains why backend uses Input DTOs (primitives) and Command objects (Vogen).

**Key Points**:

- Hot Chocolate can't deserialize Vogen value objects
- Separation of concerns: GraphQL layer vs domain layer
- Mutation resolvers map Input → Command
- Validation happens at Vogen boundary (`.From()` throws if invalid)

### Domain Model

**Location**: `docs/architecture/domain-model-microservices-map.md`

**Relevance**: Complete picture of Family bounded context and User-Family relationship.

**Lines 125-200**: Family Module

- Family aggregate root
- FamilyId, FamilyName value objects
- User → Family relationship via FamilyId (nullable)
- CreateFamily command and handler

---

## Key Insights from References

### 1. Backend is Production-Ready

The backend domain logic, GraphQL API, and command handlers are **complete and working**. We're not building new backend features—we're **wiring up existing functionality** in the frontend.

### 2. Dashboard is the Natural Integration Point

The dashboard already:

- Fetches user data (including family)
- Checks if user has a family
- Shows different UI for "has family" vs "no family"
- Has a placeholder "Create Family" button

We're just adding:

- Dialog component
- Service to call mutation
- Logic to show/hide dialog

### 3. Patterns Are Consistent Across Codebase

Every reference studied shows **consistent patterns**:

- Standalone components with `inject()`
- Angular Signals for reactive state
- Separate `.operations.ts` files for GraphQL
- Service layer between GraphQL and components
- `data-testid` attributes for E2E tests

This makes implementation straightforward—follow what exists!

### 4. Zero New Dependencies

Every pattern we need is **already demonstrated** in the codebase:

- Angular Signals ✅
- Apollo Client ✅
- Standalone components ✅
- Playwright E2E ✅

No need to introduce new libraries or patterns.

---

## Reference File Locations Summary

### Backend

- `src/FamilyHub.Api/Features/Family/Domain/Entities/Family.cs`
- `src/FamilyHub.Api/Features/Family/GraphQL/FamilyMutations.cs`
- `src/FamilyHub.Api/Features/Family/Application/Commands/CreateFamilyCommand.cs`
- `src/FamilyHub.Api/Features/Family/Models/CreateFamilyInput.cs`
- `src/FamilyHub.Api/Features/Family/Models/FamilyDto.cs`
- `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs`

### Frontend

- `src/frontend/family-hub-web/src/app/features/auth/callback/callback.component.ts`
- `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.ts`
- `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.html`
- `src/frontend/family-hub-web/src/app/core/user/user.service.ts`
- `src/frontend/family-hub-web/src/app/features/auth/graphql/auth.operations.ts`

### Testing

- `e2e/auth/oauth-complete-flow.spec.ts`

### Documentation

- `docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md` (includes Keycloak amendment)
- `docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md`
- `docs/architecture/domain-model-microservices-map.md`
- `docs/guides/FRONTEND_DEVELOPMENT.md`
- `docs/guides/BACKEND_DEVELOPMENT.md`

---

## How References Inform Implementation

### GraphQL Operations (Task 2)

**Guided by**: `auth.operations.ts`

- Same file structure
- Same export pattern
- Same `gql` syntax

### Family Service (Task 3)

**Guided by**: `user.service.ts`

- Same Injectable pattern
- Same Apollo injection
- Same Signal usage
- Same error handling (catchError)

### Dialog Component (Task 4)

**Guided by**: Dashboard component + Angular Components standard

- Standalone component
- Signal-based state
- EventEmitter for parent communication
- Tailwind-inspired utility classes

### Dashboard Integration (Task 5)

**Guided by**: Existing dashboard implementation

- Add dialog import
- Add showDialog signal
- Wire up ngOnInit logic
- Wire up button click handler

### E2E Tests (Task 6)

**Guided by**: `oauth-complete-flow.spec.ts`

- Same test structure
- Same use of `data-testid`
- Same assertion patterns
- Complete user journey approach

---

**These references provide a solid foundation for confident, consistent implementation.**
