# OAuth Login with Keycloak → Dashboard Implementation Plan

**Created:** 2026-02-03
**Status:** Approved - In Implementation

---

## Summary

Implement a complete OAuth 2.0 login flow where users authenticate with Keycloak, get synced with the backend database, and land on a dashboard showing their profile and family membership.

**Critical Discovery:** The codebase already has production-quality OAuth implementation on both frontend and backend. The only missing piece is connecting them - the frontend successfully authenticates with Keycloak but never syncs with the backend database.

---

## Implementation Tasks

1. **Save Spec Documentation** ✓ (In Progress)
2. **Create Backend GetCurrentUser Query** - `src/FamilyHub.Api/Features/Auth/GraphQL/AuthQueries.cs`
3. **Create Frontend UserService** - `src/frontend/family-hub-web/src/app/core/user/user.service.ts`
4. **Create GraphQL Operations** - `src/frontend/family-hub-web/src/app/features/auth/graphql/auth.operations.ts`
5. **Configure Apollo Auth** - `src/frontend/family-hub-web/src/app/app.config.ts`
6. **Enhance CallbackComponent** - Add backend sync after OAuth callback
7. **Enhance DashboardComponent** - Display backend user data
8. **Add E2E Test** - Playwright test for complete flow
9. **Verify JWT Config** - Ensure Keycloak JWT validation working
10. **Update Documentation** - ADR-002 amendment

---

## Success Criteria

✅ User can login with Keycloak and see dashboard with backend data
✅ Family membership displayed if user has family
✅ Logout clears both Keycloak session and backend state
✅ E2E test passes with zero retries

---

See full plan at: `/home/andrekirst/.claude/plans/rippling-hatching-dawn.md`
