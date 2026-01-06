# Family Management UI - Implementation Plan

## Overview

Implement the Family Management UI to complete Week 7 of Phase 1, providing a complete end-to-end vertical slice for the family member invitation system.

## Architecture

### Component Structure

```
features/family/
├── pages/
│   ├── family-wizard-page/          (✅ exists - wizard for creating family)
│   └── family-management-page/      (🆕 new - main family management page)
├── components/
│   ├── family-name-step/            (✅ exists - wizard step 1)
│   ├── family-members-list/         (🆕 new - shows all members with roles)
│   ├── pending-invitations/         (🆕 new - shows pending invitations)
│   ├── invite-member-modal/         (🆕 new - form for inviting members)
│   ├── credentials-display-modal/   (🆕 new - one-time credentials view)
│   └── role-badge/                  (🆕 new - reusable role display)
└── services/
    ├── family.service.ts            (✅ exists - extend with new queries/mutations)
    └── invitation.service.ts        (🆕 new - dedicated invitation operations)
```

### Data Models

```typescript
// User with family membership
export interface FamilyMember {
  id: string;
  email: string;
  emailVerified: boolean;
  role: 'OWNER' | 'ADMIN' | 'MEMBER' | 'MANAGED_ACCOUNT';
  username?: string;      // Only for managed accounts
  name?: string;      // Only for managed accounts
  auditInfo: {
    createdAt: string;
    updatedAt: string;
  };
}

// Pending invitation
export interface PendingInvitation {
  id: string;
  email?: string;         // Email invitations
  username?: string;      // Managed account invitations
  role: 'OWNER' | 'ADMIN' | 'MEMBER' | 'MANAGED_ACCOUNT';
  status: 'PENDING' | 'ACCEPTED' | 'REJECTED' | 'CANCELLED' | 'EXPIRED';
  invitedAt: string;
  expiresAt: string;
  displayCode?: string;   // Only for email invitations
}

// Credentials (one-time display only)
export interface ManagedAccountCredentials {
  username: string;
  password: string;
  syntheticEmail: string;
  loginUrl: string;
}

// Batch invite result
export interface BatchInviteResult {
  emailInvitations: PendingInvitation[];
  managedAccounts: {
    invitation: PendingInvitation;
    user: FamilyMember;
    credentials: ManagedAccountCredentials;
  }[];
}
```

## GraphQL Operations

### Queries

```graphql
# Get all family members
query GetFamilyMembers($familyId: UUID!) {
  familyMembers(familyId: $familyId) {
    id
    email
    emailVerified
    role
    username
    name
    auditInfo {
      createdAt
      updatedAt
    }
  }
}

# Get pending invitations
query GetPendingInvitations($familyId: UUID!) {
  pendingInvitations(familyId: $familyId) {
    id
    email
    username
    role
    status
    invitedAt
    expiresAt
    displayCode
  }
}
```

### Mutations

```graphql
# Batch invite members
mutation BatchInviteFamilyMembers($input: BatchInviteFamilyMembersInput!) {
  batchInviteFamilyMembers(input: $input) {
    emailInvitations {
      id
      email
      role
      displayCode
      expiresAt
    }
    managedAccounts {
      invitation {
        id
        username
        role
      }
      user {
        id
        email
        username
      }
      credentials {
        username
        password
        syntheticEmail
        loginUrl
      }
    }
    errors {
      message
      code
      field
    }
  }
}

# Cancel invitation
mutation CancelInvitation($input: CancelInvitationInput!) {
  cancelInvitation(input: $input) {
    success
    errors {
      message
      code
    }
  }
}

# Update invitation role
mutation UpdateInvitationRole($input: UpdateInvitationRoleInput!) {
  updateInvitationRole(input: $input) {
    invitation {
      id
      role
    }
    errors {
      message
      code
    }
  }
}
```

## Component Specifications

### 1. FamilyManagementPage Component

**Purpose:** Main page that combines members list, pending invitations, and invite action.

**Layout:**

```
┌─────────────────────────────────────────────────┐
│ Family Management                               │
│                                                 │
│ ┌─────────────────────────────────────────┐   │
│ │ Family Members (5)           [+ Invite] │   │
│ ├─────────────────────────────────────────┤   │
│ │ • John Doe (OWNER)                      │   │
│ │ • Jane Smith (ADMIN)                    │   │
│ │ • ...                                   │   │
│ └─────────────────────────────────────────┘   │
│                                                 │
│ ┌─────────────────────────────────────────┐   │
│ │ Pending Invitations (3)                 │   │
│ ├─────────────────────────────────────────┤   │
│ │ • bob@example.com (MEMBER) [Cancel]    │   │
│ │ • kid123 (MANAGED_ACCOUNT) [Cancel]    │   │
│ │ • ...                                   │   │
│ └─────────────────────────────────────────┘   │
└─────────────────────────────────────────────────┘
```

**Features:**

- Auto-loads members and invitations on mount
- Refresh button to reload data
- Role-based visibility (only OWNER/ADMIN can see this page)

---

### 2. FamilyMembersList Component

**Purpose:** Display all family members with their roles.

**Props:**

- `members: FamilyMember[]` - Array of family members
- `currentUserRole: string` - Current user's role (for permission checks)

**Features:**

- Role badge with color coding:
  - OWNER: purple
  - ADMIN: blue
  - MEMBER: green
  - MANAGED_ACCOUNT: gray
- Sort by role (Owner → Admin → Member → Managed Account)
- Show username for managed accounts
- Show email verification status

---

### 3. PendingInvitationsComponent

**Purpose:** Display pending invitations with cancel action.

**Props:**

- `invitations: PendingInvitation[]` - Array of pending invitations
- `onCancel: (invitationId: string) => void` - Callback for cancel action

**Features:**

- Show different displays for email vs managed account invitations
- Show expiration countdown (e.g., "Expires in 13 days")
- Cancel button (with confirmation dialog)
- Show display code for email invitations
- Filter by status (default: only PENDING)

---

### 4. InviteMemberModal Component

**Purpose:** Form for inviting new members (both email and managed accounts).

**Features:**

- Tab switcher:
  - **Email Invitations** tab
  - **Managed Accounts** tab
  - **Batch Mode** tab (allows CSV/multi-entry)

**Email Invitation Form:**

```typescript
{
  email: string;              // Required, validated
  role: 'ADMIN' | 'MEMBER';  // Required (OWNER not allowed)
  message?: string;           // Optional custom message
}
```

**Managed Account Form:**

```typescript
{
  username: string;           // Required, 3-20 chars, alphanumeric
  name: string;           // Required, 1-100 chars
  role: 'MEMBER' | 'MANAGED_ACCOUNT';  // Required
  passwordConfig: {
    length: number;           // Default: 12
    includeUppercase: boolean;  // Default: true
    includeLowercase: boolean;  // Default: true
    includeDigits: boolean;     // Default: true
    includeSymbols: boolean;    // Default: false
  };
}
```

**Validation:**

- Email format validation
- Username uniqueness check
- Role authorization check (current user must be OWNER/ADMIN)

**Actions:**

- Submit → calls `batchInviteFamilyMembers` mutation
- Cancel → closes modal
- On success with managed account → opens CredentialsDisplayModal

---

### 5. CredentialsDisplayModal Component

**Purpose:** One-time display of generated credentials for managed accounts.

**Critical Security Features:**

- ⚠️ **ONE-TIME VIEW ONLY** - credentials are NEVER retrievable after closing
- Cannot be reopened after closing
- Print button for physical copy
- Copy to clipboard buttons for each field
- Large warning banner explaining one-time nature

**Layout:**

```
┌───────────────────────────────────────────────┐
│ ⚠️ IMPORTANT: Save These Credentials Now     │
│                                               │
│ These credentials will only be shown ONCE.   │
│ Please save them securely before closing.    │
│                                               │
│ Username:        kid123            [Copy]    │
│ Password:        X8f$mK2p@qR9      [Copy]    │
│ Synthetic Email: kid123@noemail... [Copy]    │
│ Login URL:       https://auth...   [Copy]    │
│                                               │
│              [Print]  [I Have Saved This]    │
└───────────────────────────────────────────────┘
```

**Props:**

- `credentials: ManagedAccountCredentials` - Credentials to display
- `username: string` - Associated username
- `onClose: () => void` - Callback when closed

**Features:**

- Cannot be closed via ESC key or backdrop click (must click "I Have Saved This")
- Confirmation dialog before closing
- Print view optimized for physical storage

---

### 6. RoleBadge Component

**Purpose:** Reusable component for displaying user roles with consistent styling.

**Props:**

- `role: 'OWNER' | 'ADMIN' | 'MEMBER' | 'MANAGED_ACCOUNT'` - Role to display
- `size?: 'sm' | 'md' | 'lg'` - Size variant (default: 'md')

**Styling:**

```typescript
const roleStyles = {
  OWNER:           'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300',
  ADMIN:           'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
  MEMBER:          'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
  MANAGED_ACCOUNT: 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300'
};
```

## Permission System

### AuthGuard Enhancement

Create `RoleGuard` to restrict access based on user role:

```typescript
export class RoleGuard {
  canActivate(route: ActivatedRouteSnapshot): boolean {
    const requiredRoles = route.data['roles'] as string[];
    const userRole = this.authService.currentUser()?.role;

    return requiredRoles.includes(userRole);
  }
}
```

**Route Configuration:**

```typescript
{
  path: 'family/manage',
  component: FamilyManagementPage,
  canActivate: [AuthGuard, RoleGuard],
  data: { roles: ['OWNER', 'ADMIN'] }
}
```

### Directive for UI Elements

Create `*hasRole` structural directive for conditional rendering:

```typescript
@Directive({ selector: '[hasRole]' })
export class HasRoleDirective {
  @Input() hasRole!: string | string[];

  // Shows element only if user has one of the specified roles
}
```

**Usage:**

```html
<button *hasRole="['OWNER', 'ADMIN']" (click)="inviteMember()">
  Invite Member
</button>
```

## Services Implementation

### InvitationService

```typescript
@Injectable({ providedIn: 'root' })
export class InvitationService {
  private graphqlService = inject(GraphQLService);

  pendingInvitations = signal<PendingInvitation[]>([]);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  async loadPendingInvitations(familyId: string): Promise<void> { }

  async batchInviteMembers(input: BatchInviteInput): Promise<BatchInviteResult> { }

  async cancelInvitation(invitationId: string): Promise<void> { }

  async updateInvitationRole(invitationId: string, newRole: string): Promise<void> { }
}
```

### Extended FamilyService

Add to existing FamilyService:

```typescript
export class FamilyService {
  // ... existing code ...

  familyMembers = signal<FamilyMember[]>([]);

  async loadFamilyMembers(familyId: string): Promise<void> { }
}
```

## Testing Strategy

### Unit Tests

- [ ] FamilyMembersList component rendering
- [ ] PendingInvitations component rendering
- [ ] InviteMemberModal form validation
- [ ] CredentialsDisplayModal security features
- [ ] RoleBadge styling variants
- [ ] InvitationService GraphQL operations
- [ ] RoleGuard permission logic

### Integration Tests

- [ ] FamilyManagementPage loads data on mount
- [ ] Invite flow completes successfully
- [ ] Cancel invitation works
- [ ] Role-based visibility enforcement

### E2E Tests (Playwright)

- [ ] Complete email invitation workflow
- [ ] Complete managed account creation workflow
- [ ] Credentials display and save workflow
- [ ] Cancel pending invitation
- [ ] Permission enforcement (non-admin cannot access)

## Implementation Order

### Phase 1: Services & Models (1-2 hours)

1. ✅ Define TypeScript interfaces
2. ✅ Create InvitationService
3. ✅ Extend FamilyService with member queries
4. ✅ Write service unit tests

### Phase 2: Core Components (3-4 hours)

1. ✅ Create RoleBadge component
2. ✅ Create FamilyMembersList component
3. ✅ Create PendingInvitationsComponent
4. ✅ Write component unit tests

### Phase 3: Modals (2-3 hours)

1. ✅ Create InviteMemberModal component
2. ✅ Create CredentialsDisplayModal component
3. ✅ Write modal unit tests

### Phase 4: Page Integration (2-3 hours)

1. ✅ Create FamilyManagementPage
2. ✅ Wire up all components
3. ✅ Add routing
4. ✅ Write integration tests

### Phase 5: Permissions (1-2 hours)

1. ✅ Create RoleGuard
2. ✅ Create *hasRole directive
3. ✅ Apply guards to routes
4. ✅ Write permission tests

### Phase 6: E2E Testing (2-3 hours)

1. ✅ Write invitation E2E tests
2. ✅ Write credentials display E2E test
3. ✅ Write permission enforcement E2E test

### Phase 7: Polish & Documentation (1-2 hours)

1. ✅ Add loading states
2. ✅ Add error handling
3. ✅ Add accessibility labels
4. ✅ Update README documentation

**Total Estimated Effort:** 12-20 hours

## Success Criteria

✅ **Functional:**

- Owner/Admin can view all family members with roles
- Owner/Admin can view pending invitations
- Owner/Admin can invite members via email or managed account
- Credentials for managed accounts display only once
- Non-owners/admins cannot access family management page

✅ **Technical:**

- All unit tests passing (>80% coverage)
- All integration tests passing
- All E2E tests passing
- TypeScript strict mode enabled
- No console errors or warnings

✅ **UX:**

- Loading states for all async operations
- Error messages for failed operations
- Confirmation dialogs for destructive actions
- Responsive design (mobile, tablet, desktop)
- WCAG 2.1 AA accessibility compliance

## Next Steps After Completion

After completing the Family Management UI (Week 7), the next phase is:

**Weeks 8-10: Calendar Service**

- CalendarEvent aggregate (backend)
- Calendar view UI (FullCalendar integration)
- Event creation and editing
- Family-wide event sharing
- Foundation for event chains in Phase 2
