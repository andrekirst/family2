# Mandatory Family Onboarding Gate

**Created**: 2026-03-11
**GitHub Issue**: #229
**Spec**: `agent-os/specs/2026-03-11-family-onboarding-gate/`

## Context

Users who log in without a family currently see a temporary "Create Family" button in the sidebar that creates a family with a hardcoded name "My Family". This needs to be replaced with a proper mandatory onboarding gate: a full-screen page (no sidebar/header) that requires users to either create a named family or accept a pending invitation before accessing any app features.

## Files to Modify

| File | Action |
|------|--------|
| `src/frontend/.../core/auth/auth.guard.ts` | Add `noFamilyGuard`, update `familyMemberGuard` redirect |
| `src/frontend/.../features/family/components/family-onboarding/family-onboarding.component.ts` | **NEW** — Full-screen onboarding gate |
| `src/frontend/.../app.routes.ts` | Add onboarding route outside layout shell, guard dashboard |
| `src/frontend/.../shared/layout/sidebar/sidebar.component.ts` | Remove temporary Create Family button |
| `src/frontend/.../features/auth/callback/callback.component.ts` | Direct redirect to onboarding when no family |

## Verification

1. **Build check**: `cd src/frontend/family-hub-web && npx ng build`
2. Login without family -> redirect to `/family/onboarding`
3. Create family -> redirect to `/dashboard`
4. Login with family -> direct to `/dashboard`
5. Navigate to `/family/onboarding` with family -> redirect to `/dashboard`
6. Accept invitation on onboarding -> redirect to `/dashboard`
7. Decline invitation -> removed from list, stays on onboarding
8. Navigate to `/calendar` without family -> redirect to `/family/onboarding`
