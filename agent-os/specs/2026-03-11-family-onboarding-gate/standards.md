# Standards for Mandatory Family Onboarding Gate

The following standards apply to this work.

---

## frontend/angular-components

All components are standalone (no NgModules). Use `inject()` for DI and Angular Signals for state.

Key rules:

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state (`signal()`, `computed()`)
- Use `ChangeDetectionStrategy.OnPush`

---

## frontend/apollo-graphql

Use Apollo Client for GraphQL with typed operations.

Key rules:

- Use `inject(Apollo)` for dependency injection
- Handle errors with `catchError`
- Use typed operations (gql tagged templates)
- Use `fetchPolicy: 'network-only'` for data that must be fresh

---

## backend/graphql-input-command

No backend changes needed for this feature. All required queries and mutations already exist:

- `family.create(input)` — CreateFamily mutation
- `me.invitations.getPendings()` — GetMyPendingInvitations query
- `family.invitation.acceptById(id)` — AcceptInvitationById mutation
- `family.invitation.declineById(id)` — DeclineInvitationById mutation

---

## backend/permission-system

The onboarding page is pre-family — no permission checks are needed since the user has no family role yet. The existing guards (`authGuard`, `noFamilyGuard`) handle access control.

After creating a family or accepting an invitation, the user receives an Owner or invited role respectively, and permissions are populated via `GetCurrentUser` query.
