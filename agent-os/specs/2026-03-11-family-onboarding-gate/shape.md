# Mandatory Family Onboarding Gate — Shaping Notes

**Feature**: Full-screen mandatory family onboarding gate after login
**Created**: 2026-03-11
**GitHub Issue**: #229

---

## Scope

Replace the temporary "Create Family" button in the sidebar with a proper mandatory onboarding flow. When a user logs in and has no family assigned (`familyId: null`), they must be redirected to a full-screen onboarding page where they can either:

1. **Create a new family** by providing a name
2. **Accept a pending invitation** to join an existing family

The user cannot access any app features (dashboard, calendar, etc.) until they are assigned to a family. No skip option.

## Decisions

- **Full-screen gate** — Onboarding route placed outside the `LayoutComponent` shell (like `login`, `callback`, `invitation/accept`), so no sidebar or header is rendered
- **Strictly mandatory** — No skip/dismiss option; `familyMemberGuard` on dashboard + all feature routes enforces this
- **`noFamilyGuard`** — Inverse guard prevents users WITH a family from accessing the onboarding page (handles manual URL navigation, stale bookmarks)
- **Reuse existing services** — `FamilyService.createFamily()`, `InvitationService.getMyPendingInvitations()`, `.acceptInvitationById()`, `.declineInvitationById()` are all already implemented
- **No backend changes** — All required queries and mutations already exist
- **Smoother redirect** — `CallbackComponent` checks `familyId` after `registerUser()` and navigates directly to onboarding, avoiding dashboard -> onboarding flash

## Context

- **Visuals:** None (ASCII mockup from shaping conversation used as guide)
- **References:** `familyMemberGuard` redirect pattern, `invitation-accept.component.ts` full-screen layout, `create-family-dialog.component.ts` form pattern
- **Product alignment:** Phase 1 - Core MVP. Family assignment is a prerequisite for all family-scoped features.

## Standards Applied

- frontend/angular-components — Standalone component with signals, `inject()` DI
- frontend/apollo-graphql — Apollo Client with typed operations for GraphQL calls
- backend/graphql-input-command — No backend changes needed, existing pattern sufficient
- backend/permission-system — No permission checks needed (onboarding is pre-family)
