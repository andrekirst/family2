# Family Creation Post-Login Feature - Implementation Plan

**Created**: 2026-02-03
**Spec Folder**: `agent-os/specs/2026-02-03-1430-family-creation-post-login/`

---

## Overview

**User Story**: As a user, when I successfully log in, the system checks if I'm assigned to a family. If not, I see an optional dialog/form to create a family by providing a family name. I can dismiss the dialog and create the family later from the dashboard.

**Current State**:

- ✅ Backend: `CreateFamily` GraphQL mutation exists and works
- ✅ Backend: User entity has nullable `FamilyId` field
- ✅ Frontend: Dashboard shows family status and has placeholder "Create Family" button
- ✅ Frontend: `GetCurrentUser` query includes family data
- ❌ Frontend: No dialog/modal component for family creation
- ❌ Frontend: Dialog doesn't show post-login
- ❌ E2E: No tests for family creation flow

**What We're Building**:

1. Reusable "Create Family" dialog component (Angular)
2. Family service to handle GraphQL mutations
3. Integration with dashboard to show dialog conditionally
4. E2E tests for the complete flow

---

## Critical Files

### Backend (Reference Only - No Changes)

- `src/FamilyHub.Api/Features/Family/GraphQL/FamilyMutations.cs` - CreateFamily mutation
- `src/FamilyHub.Api/Features/Family/Domain/Entities/Family.cs` - Family aggregate
- `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs` - User with FamilyId

### Frontend (Will Modify/Create)

- `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.ts` - Wire up dialog
- `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.html` - Update template
- **NEW**: `src/frontend/family-hub-web/src/app/features/family/components/create-family-dialog/` - Dialog component
- **NEW**: `src/frontend/family-hub-web/src/app/features/family/services/family.service.ts` - GraphQL service
- **NEW**: `src/frontend/family-hub-web/src/app/features/family/graphql/family.operations.ts` - GraphQL operations

### Testing (Will Create)

- **NEW**: `e2e/family/family-creation-post-login.spec.ts` - E2E test

---

## Implementation Tasks

### Task 1: Save Spec Documentation

**Create**: `agent-os/specs/2026-02-03-1430-family-creation-post-login/`

**Files to Generate**:

1. **plan.md** - This full plan document
2. **shape.md** - Shaping decisions and context from our conversation:
   - Scope: Optional post-login family creation dialog
   - User can dismiss and create later
   - Dashboard shows family status
   - Backend mutation already exists
3. **standards.md** - Full content of 4 relevant standards:
   - `backend/graphql-input-command.md`
   - `frontend/angular-components.md`
   - `frontend/apollo-graphql.md`
   - `testing/playwright-e2e.md`
4. **references.md** - References studied:
   - Auth flow: `CallbackComponent`, `DashboardComponent`, `UserService`
   - Family domain: `Family.cs`, `FamilyMutations.cs`, `CreateFamilyCommand`
   - E2E pattern: `e2e/auth/oauth-complete-flow.spec.ts`

**Deliverable**: Spec folder created with complete documentation before implementation begins.

---

### Task 2: Create Family GraphQL Operations

**Location**: `src/frontend/family-hub-web/src/app/features/family/graphql/family.operations.ts`

**Create GraphQL mutation** for family creation:

```typescript
import { gql } from 'apollo-angular';

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

**Why**: Separate GraphQL operations from component logic (separation of concerns). Follows Apollo GraphQL standard.

**Pattern**: Matches existing `auth.operations.ts` structure in the codebase.

---

### Task 3: Create Family Service

**Location**: `src/frontend/family-hub-web/src/app/features/family/services/family.service.ts`

**Create service** to handle family creation:

```typescript
import { Injectable, inject, signal } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { CREATE_FAMILY } from '../graphql/family.operations';
import { catchError, map, of } from 'rxjs';

export interface CreateFamilyInput {
  name: string;
}

export interface FamilyDto {
  id: string;
  name: string;
  ownerId: string;
  createdAt: string;
  memberCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class FamilyService {
  private apollo = inject(Apollo);

  createFamily(input: CreateFamilyInput) {
    return this.apollo.mutate<{ createFamily: FamilyDto }>({
      mutation: CREATE_FAMILY,
      variables: { input }
    }).pipe(
      map(result => result.data?.createFamily),
      catchError(error => {
        console.error('Failed to create family:', error);
        return of(null);
      })
    );
  }
}
```

**Why**:

- Centralized family operations (single source of truth)
- Reusable across multiple components
- Proper error handling with RxJS operators
- Follows Apollo GraphQL standard

**Pattern**: Matches `UserService` structure in the codebase.

---

### Task 4: Create Family Dialog Component

**Location**: `src/frontend/family-hub-web/src/app/features/family/components/create-family-dialog/`

**Files**:

- `create-family-dialog.component.ts`
- `create-family-dialog.component.html`
- `create-family-dialog.component.css`

**Component Structure** (standalone component with Angular Signals):

```typescript
import { Component, EventEmitter, Output, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FamilyService } from '../../services/family.service';

@Component({
  selector: 'app-create-family-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-family-dialog.component.html',
  styleUrls: ['./create-family-dialog.component.css']
})
export class CreateFamilyDialogComponent {
  private familyService = inject(FamilyService);

  @Output() familyCreated = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  familyName = signal('');
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  onSubmit() {
    if (!this.familyName().trim()) {
      this.errorMessage.set('Family name is required');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.familyService.createFamily({ name: this.familyName() })
      .subscribe({
        next: (family) => {
          if (family) {
            this.familyCreated.emit();
          } else {
            this.errorMessage.set('Failed to create family');
          }
          this.isLoading.set(false);
        },
        error: (error) => {
          this.errorMessage.set('An error occurred');
          this.isLoading.set(false);
        }
      });
  }

  onDismiss() {
    this.dialogClosed.emit();
  }
}
```

**Template** (simple, accessible form):

```html
<div class="dialog-overlay" (click)="onDismiss()">
  <div class="dialog-content" (click)="$event.stopPropagation()">
    <div class="dialog-header">
      <h2>Create Your Family</h2>
      <button class="close-button" (click)="onDismiss()" aria-label="Close">×</button>
    </div>

    <div class="dialog-body">
      <p>Get started by creating your family. You can invite members later.</p>

      <form (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label for="family-name">Family Name</label>
          <input
            id="family-name"
            type="text"
            data-testid="family-name-input"
            [(ngModel)]="familyName"
            [disabled]="isLoading()"
            name="familyName"
            placeholder="e.g., Smith Family"
            class="form-control"
          />
        </div>

        @if (errorMessage()) {
          <div class="error-message" role="alert">
            {{ errorMessage() }}
          </div>
        }

        <div class="dialog-actions">
          <button
            type="button"
            (click)="onDismiss()"
            [disabled]="isLoading()"
            class="btn btn-secondary"
          >
            Skip for Now
          </button>
          <button
            type="submit"
            data-testid="create-family-button"
            [disabled]="isLoading()"
            class="btn btn-primary"
          >
            {{ isLoading() ? 'Creating...' : 'Create Family' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</div>
```

**Styles** (simple modal overlay):

```css
.dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.dialog-content {
  background: white;
  border-radius: 8px;
  max-width: 500px;
  width: 90%;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.dialog-header h2 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
}

.close-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #6b7280;
}

.dialog-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-control {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 1rem;
}

.form-control:disabled {
  background-color: #f3f4f6;
  cursor: not-allowed;
}

.error-message {
  color: #dc2626;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

.dialog-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 4px;
  font-weight: 500;
  cursor: pointer;
  border: none;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background-color: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #2563eb;
}

.btn-secondary {
  background-color: #e5e7eb;
  color: #374151;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #d1d5db;
}
```

**Why**:

- Standalone component (follows Angular Components standard)
- Uses Angular Signals for reactive state
- Accessible (ARIA labels, keyboard navigation)
- Dismissible via "Skip for Now" button or overlay click
- Loading and error states
- Event emitters for parent component communication

**Pattern**: Follows atomic design (organism level). Similar to modal patterns in modern Angular apps.

---

### Task 5: Integrate Dialog with Dashboard

**Modify**: `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.ts`

**Changes**:

1. Import the dialog component
2. Add signal to control dialog visibility
3. Show dialog on component init if user has no family
4. Handle dialog events (created, closed)
5. Refresh user data after family creation

```typescript
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../core/user/user.service';
import { CreateFamilyDialogComponent } from '../family/components/create-family-dialog/create-family-dialog.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CreateFamilyDialogComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  private userService = inject(UserService);

  currentUser = this.userService.currentUser;
  showCreateFamilyDialog = signal(false);

  ngOnInit(): void {
    // Show dialog if user has no family (optional)
    if (this.currentUser() && !this.currentUser()!.family) {
      // Small delay for better UX (user sees dashboard first)
      setTimeout(() => {
        this.showCreateFamilyDialog.set(true);
      }, 500);
    }
  }

  onFamilyCreated() {
    this.showCreateFamilyDialog.set(false);
    // Refresh user data to get updated family info
    this.userService.fetchCurrentUser().subscribe();
  }

  onDialogClosed() {
    this.showCreateFamilyDialog.set(false);
  }

  openCreateFamilyDialog() {
    this.showCreateFamilyDialog.set(true);
  }
}
```

**Modify**: `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.component.html`

**Changes**:

1. Add dialog component with conditional rendering
2. Wire up existing "Create Family" button to open dialog

```html
<!-- Existing content -->
<div class="dashboard-container">
  <h1>Welcome, {{ currentUser()!.name }}</h1>
  <p>Email: {{ currentUser()!.email }}</p>

  @if (currentUser()!.family) {
    <div class="mt-6 p-4 bg-blue-50 rounded-lg">
      <h3 class="text-lg font-medium text-gray-900">
        Family: {{ currentUser()!.family!.name }}
      </h3>
      <p class="mt-1 text-sm text-gray-600">
        You are part of this family. View details or manage members.
      </p>
      <button class="mt-3 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600">
        View Family
      </button>
    </div>
  } @else {
    <div class="mt-6 p-4 bg-gray-50 rounded-lg">
      <h3 class="text-lg font-medium text-gray-900">No Family Yet</h3>
      <p class="mt-1 text-sm text-gray-600">
        Create your family to start organizing your life together.
      </p>
      <button
        (click)="openCreateFamilyDialog()"
        class="mt-3 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600"
      >
        Create Family
      </button>
    </div>
  }
</div>

<!-- NEW: Create Family Dialog -->
@if (showCreateFamilyDialog()) {
  <app-create-family-dialog
    (familyCreated)="onFamilyCreated()"
    (dialogClosed)="onDialogClosed()"
  />
}
```

**Why**:

- Automatic dialog on first login (optional, dismissible)
- Manual trigger from dashboard button
- Refreshes user data after creation (ensures UI updates)
- Clean separation of concerns (dialog component is reusable)

---

### Task 6: Create E2E Test

**Location**: `e2e/family/family-creation-post-login.spec.ts`

**Test Suite**:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Family Creation Post-Login', () => {
  test.beforeEach(async ({ page }) => {
    // Assumption: Test helpers exist for OAuth login
    // This would typically use a test user that doesn't have a family
    await page.goto('/');
  });

  test('shows create family dialog when user has no family', async ({ page }) => {
    // Step 1: Login as user without family
    await page.click('[data-testid="login-button"]');

    // Wait for OAuth redirect and callback
    // (Implementation depends on your OAuth test setup)
    await page.waitForURL('/dashboard');

    // Step 2: Verify dialog appears after delay
    await page.waitForSelector('[data-testid="family-name-input"]', { timeout: 2000 });

    const dialogVisible = await page.isVisible('[data-testid="family-name-input"]');
    expect(dialogVisible).toBe(true);
  });

  test('creates family from post-login dialog', async ({ page }) => {
    // Step 1: Login and wait for dialog
    await page.goto('/dashboard'); // Assume already authenticated
    await page.waitForSelector('[data-testid="family-name-input"]');

    // Step 2: Fill in family name
    await page.fill('[data-testid="family-name-input"]', 'Test Family');

    // Step 3: Submit form
    await page.click('[data-testid="create-family-button"]');

    // Step 4: Verify dialog closes and family appears on dashboard
    await expect(page.locator('[data-testid="family-name-input"]')).not.toBeVisible();
    await expect(page.locator('text=Family: Test Family')).toBeVisible();
  });

  test('dismisses dialog and allows manual creation later', async ({ page }) => {
    // Step 1: Login and wait for dialog
    await page.goto('/dashboard');
    await page.waitForSelector('[data-testid="family-name-input"]');

    // Step 2: Click "Skip for Now"
    await page.click('text=Skip for Now');

    // Step 3: Verify dialog closes
    await expect(page.locator('[data-testid="family-name-input"]')).not.toBeVisible();

    // Step 4: Verify "Create Family" button is still available
    await expect(page.locator('text=Create Family')).toBeVisible();

    // Step 5: Click button to reopen dialog
    await page.click('text=Create Family');

    // Step 6: Verify dialog reopens
    await expect(page.locator('[data-testid="family-name-input"]')).toBeVisible();
  });

  test('shows error when family name is empty', async ({ page }) => {
    // Step 1: Open dialog
    await page.goto('/dashboard');
    await page.waitForSelector('[data-testid="family-name-input"]');

    // Step 2: Submit without entering name
    await page.click('[data-testid="create-family-button"]');

    // Step 3: Verify error message
    await expect(page.locator('text=Family name is required')).toBeVisible();
  });
});
```

**Why**:

- Zero retry policy (follows Playwright E2E standard)
- Tests complete user journey (login → dialog → creation)
- Tests dismissal and manual creation flow
- Tests validation (empty name)
- Uses data-testid selectors (best practice)

**Note**: This test assumes OAuth test helpers exist. You may need to adjust based on your authentication test setup.

---

### Task 7: Update User Service to Refresh Family Data

**Modify**: `src/frontend/family-hub-web/src/app/core/user/user.service.ts`

**Ensure** `fetchCurrentUser()` is public and refetches user data (it should already be implemented):

```typescript
fetchCurrentUser() {
  return this.apollo.query<{ currentUser: CurrentUser }>({
    query: GET_CURRENT_USER,
    fetchPolicy: 'network-only' // Force refetch from server
  }).pipe(
    map(result => {
      this.currentUser.set(result.data.currentUser);
      return result.data.currentUser;
    }),
    catchError(error => {
      console.error('Failed to fetch current user:', error);
      return of(null);
    })
  );
}
```

**Why**: After family creation, we need to refresh user data to get the updated family relationship. Using `fetchPolicy: 'network-only'` ensures we don't get stale cached data.

**Pattern**: Matches existing `UserService` implementation.

---

## Verification Plan

### Manual Testing Steps

1. **Clear browser storage** and logout
2. **Login** with a new user (or existing user without family)
3. **Verify** dialog appears ~500ms after dashboard loads
4. **Dismiss** dialog via "Skip for Now" button
5. **Verify** dashboard shows "No Family Yet" section
6. **Click** "Create Family" button
7. **Verify** dialog reopens
8. **Enter** family name (e.g., "Smith Family")
9. **Click** "Create Family" button
10. **Verify** dialog closes
11. **Verify** dashboard now shows "Family: Smith Family"
12. **Refresh** page
13. **Verify** no dialog appears (user already has family)

### E2E Test Execution

```bash
cd e2e
npx playwright test family/family-creation-post-login.spec.ts
```

**Expected**: All 4 tests pass across chromium, firefox, webkit.

### Backend Verification (Already Working)

Use GraphQL Playground to verify mutation:

```graphql
mutation {
  createFamily(input: { name: "Test Family" }) {
    id
    name
    ownerId
    createdAt
    memberCount
  }
}
```

**Expected**: Returns created family with ID.

---

## Architecture Insights

### Domain-Driven Design

- **Family** is an aggregate root in the Family bounded context
- **User** is an aggregate root in the Auth bounded context
- User → Family relationship is established via `FamilyId` foreign key
- Family creation triggers a domain event (if event-driven architecture is enabled)

### GraphQL Input→Command Pattern (ADR-003)

The backend follows this pattern:

1. **GraphQL Input** (`CreateFamilyInput`) uses primitives (`string`)
2. **MediatR Command** (`CreateFamilyCommand`) uses Vogen value objects (`FamilyName`)
3. **Mutation resolver** maps Input → Command

This separation keeps Hot Chocolate (GraphQL) layer separate from domain layer.

### Frontend Architecture

- **Standalone components** (no NgModules)
- **Angular Signals** for reactive state
- **Apollo Client** for GraphQL
- **Service layer** separates GraphQL operations from UI logic
- **Event emitters** for parent-child communication

---

## Dependencies

### Backend

- ✅ Already implemented (no new dependencies)

### Frontend

- ✅ `@angular/core` (already in project)
- ✅ `@angular/common` (already in project)
- ✅ `@angular/forms` (already in project)
- ✅ `apollo-angular` (already in project)
- ✅ `graphql-tag` (already in project)

### Testing

- ✅ `@playwright/test` (already in project)

**No new package installations required.**

---

## Rollout Strategy

### Phase 1: Core Implementation (This Plan)

- Create dialog component
- Wire up dashboard integration
- Add E2E tests

### Phase 2: Future Enhancements (Not in Scope)

- Add family settings page
- Allow family renaming
- Add member invitation flow
- Add family deletion/leave flow

---

## Success Criteria

✅ User without family sees optional dialog after login
✅ User can create family by entering name
✅ User can dismiss dialog and create later
✅ Dashboard updates to show family after creation
✅ Dialog doesn't reappear for users with family
✅ All E2E tests pass (zero retries)
✅ Code follows project standards (Angular Components, Apollo GraphQL, Playwright E2E)
✅ Spec documentation saved before implementation

---

## Educational Insights

### Why This Architecture Works

1. **Separation of Concerns**:
   - GraphQL operations isolated in `.operations.ts`
   - Business logic in services
   - UI logic in components
   - Each layer has a single responsibility

2. **Reusability**:
   - Dialog component can be used anywhere (not coupled to dashboard)
   - Family service can be injected into any component
   - GraphQL operations can be shared across features

3. **Testability**:
   - Service layer can be mocked in component tests
   - E2E tests cover user journey end-to-end
   - Zero retry policy ensures tests are reliable, not flaky

4. **DDD Alignment**:
   - User and Family are separate aggregates
   - Relationship managed via foreign key (FamilyId)
   - Backend enforces invariants (family name required, user must exist)

5. **Standards Adherence**:
   - **Backend**: Input→Command pattern keeps GraphQL separate from domain
   - **Frontend**: Standalone components, Signals, Apollo Client patterns
   - **Testing**: API-first setup, zero retries, data-testid selectors

---

**Plan Ready for Execution**

This plan provides a complete, step-by-step implementation guide for adding post-login family creation with an optional dialog. The feature integrates seamlessly with existing authentication and family domain logic.
