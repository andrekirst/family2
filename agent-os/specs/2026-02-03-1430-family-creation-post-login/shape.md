# Family Creation Post-Login — Shaping Notes

**Feature**: Optional dialog/form for creating a family after successful login

**Created**: 2026-02-03

---

## Scope

When a user successfully logs in, the system checks if they're assigned to a family. If not, they see an **optional** dialog/form to create a family by providing a family name. The user can dismiss the dialog and create the family later from the dashboard.

### Key Requirements

1. **Post-login check**: After successful authentication, check if `user.family` is null
2. **Optional dialog**: Show a modal dialog with a form to create family
3. **Dismissible**: User can click "Skip for Now" to dismiss the dialog
4. **Manual trigger**: Dashboard button allows reopening the dialog later
5. **Family name input**: Simple text input for family name
6. **Backend integration**: Use existing `CreateFamily` GraphQL mutation
7. **Post-creation redirect**: After creation, user stays on dashboard with updated family info

---

## Decisions

### User Flow Decision

**Question**: Should this family creation happen automatically (modal pops up immediately after login), or should users have the option to dismiss and create later?

**Answer**: **Optional - Can dismiss**

- User can skip and create family later from settings/dashboard
- Better UX - not forcing users into an action
- Small delay (500ms) before showing dialog so user sees dashboard first

### Post-Creation Behavior

**Question**: What happens after the family is created? Where should the user be redirected?

**Answer**: **Dashboard/Home page**

- User stays on dashboard after family creation
- Dashboard automatically refreshes to show family info
- No additional setup wizard at this stage

### Implementation Approach

**Question**: Where should the family creation check be triggered?

**Answer**: **Dashboard component**

- Check in `ngOnInit()` of `DashboardComponent`
- Show dialog after 500ms delay (better UX)
- Dialog component is reusable (can be used anywhere, not just post-login)

---

## Context

### Visuals

**Status**: No mockups provided

- Using standard dialog/modal patterns from modern Angular apps
- Simple, accessible form design with minimal styling
- Tailwind-inspired utility classes (matching existing dashboard styles)

### References

**Studied**:

1. **Auth flow**:
   - `CallbackComponent` - OAuth callback handling
   - `DashboardComponent` - Post-login landing page
   - `UserService` - Backend user state management
   - Confirmed: User data includes `family` field (nullable)

2. **Family domain**:
   - `Family.cs` - Family aggregate root
   - `FamilyMutations.cs` - CreateFamily mutation (already implemented!)
   - `CreateFamilyCommand` - MediatR command pattern
   - Confirmed: Backend logic is complete and working

3. **E2E patterns**:
   - `e2e/auth/oauth-complete-flow.spec.ts` - Reference for auth testing
   - Zero retry policy
   - API-first test setup

### Product Alignment

**Status**: No product folder exists (N/A)

- Feature aligns with core family organization concept
- Privacy-first approach (optional, not forced)
- Fits naturally into post-login onboarding flow

---

## Standards Applied

Based on analysis, these 4 standards apply to this work:

### 1. backend/graphql-input-command

**Why it applies**:

- Family creation uses GraphQL mutation
- Backend follows Input→Command pattern
- Separates GraphQL layer (primitives) from domain layer (Vogen value objects)

**How we use it**:

- Frontend sends `CreateFamilyInput` with `name: string`
- Backend maps to `CreateFamilyCommand` with `FamilyName` value object
- No backend changes needed (already follows pattern)

### 2. frontend/angular-components

**Why it applies**:

- Building new dialog component
- Must follow standalone component pattern
- Use Angular Signals for state management

**How we use it**:

- `CreateFamilyDialogComponent` is standalone (no NgModules)
- Uses Signals for `familyName`, `isLoading`, `errorMessage`
- Follows atomic design (organism level)

### 3. frontend/apollo-graphql

**Why it applies**:

- Need to call GraphQL mutation from Angular
- Must use Apollo Client patterns
- Proper error handling with RxJS

**How we use it**:

- Create `family.operations.ts` with GraphQL mutation
- `FamilyService` uses `inject(Apollo)` and `apollo.mutate()`
- Error handling with `catchError()` operator

### 4. testing/playwright-e2e

**Why it applies**:

- Must test complete user flow end-to-end
- Follow zero retry policy
- Use data-testid selectors

**How we use it**:

- Create `family-creation-post-login.spec.ts`
- Test scenarios: auto-show, create, dismiss, validation
- Zero retries, API-first setup, multi-browser support

---

## Technical Constraints

1. **No backend changes needed**: CreateFamily mutation already exists and works
2. **No new dependencies**: All required packages already in project
3. **Reusable component**: Dialog must be reusable (not coupled to dashboard)
4. **Accessibility**: Must support keyboard navigation, ARIA labels
5. **Loading states**: Must handle async operations gracefully
6. **Error handling**: Must display validation and network errors

---

## Success Indicators

How do we know this feature is successful?

1. **Functional**:
   - ✅ Dialog appears for users without family
   - ✅ Dialog can be dismissed
   - ✅ Family can be created with name input
   - ✅ Dashboard updates after creation
   - ✅ Dialog doesn't reappear after creation

2. **Quality**:
   - ✅ All E2E tests pass (zero retries)
   - ✅ Code follows project standards
   - ✅ Component is reusable
   - ✅ Accessible to keyboard/screen reader users

3. **Documentation**:
   - ✅ Spec documentation captured
   - ✅ Implementation matches plan
   - ✅ Future developers can understand decisions

---

## Future Considerations (Out of Scope)

These are potential enhancements for future iterations:

1. **Family settings page**: Dedicated page for managing family
2. **Family renaming**: Allow changing family name
3. **Member invitations**: Invite other users to join family
4. **Family deletion/leaving**: Users can leave or delete families
5. **Multi-family support**: Users belong to multiple families
6. **Family avatar/photo**: Visual identity for families

---

## Risks & Mitigations

### Risk: Dialog feels intrusive

**Mitigation**:

- 500ms delay before showing
- Clear "Skip for Now" button
- User can always access via dashboard button

### Risk: User creates family but doesn't see it

**Mitigation**:

- Refresh user data immediately after creation
- Use `fetchPolicy: 'network-only'` to avoid cache issues
- Dashboard conditionally renders family section

### Risk: Network error during creation

**Mitigation**:

- Show clear error message
- Keep dialog open so user can retry
- Log errors to console for debugging

### Risk: E2E tests are flaky

**Mitigation**:

- Zero retry policy (forces us to fix root causes)
- Use data-testid selectors (more stable than text/CSS)
- API-first setup (consistent test data)

---

## Notes from Exploration

### Current State (Before Implementation)

**Backend**: ✅ Complete

- User entity has nullable `FamilyId` field
- Family entity exists as aggregate root
- CreateFamily mutation works
- Returns FamilyDto with id, name, ownerId, createdAt, memberCount

**Frontend**: ⚠️ Partially Complete

- Dashboard already checks if user has family
- Shows "No Family Yet" message
- Has placeholder "Create Family" button (not wired up)
- GetCurrentUser query includes family data

**What's Missing**: ❌

- Dialog component doesn't exist
- Button doesn't do anything
- No E2E tests for this flow

### Key Insight

**The backend is already perfect for this feature!** We're essentially just adding UI on top of existing, working domain logic. This significantly reduces risk and implementation complexity.

---

## Timeline

**Estimated Implementation**: 3-4 hours

- Task 1 (Spec docs): 30 min
- Task 2 (GraphQL ops): 15 min
- Task 3 (Service): 30 min
- Task 4 (Dialog component): 1.5 hours
- Task 5 (Dashboard integration): 30 min
- Task 6 (E2E tests): 45 min
- Task 7 (Verify service): 15 min

**Note**: This is an estimate for reference only. Focus is on quality, not speed.
