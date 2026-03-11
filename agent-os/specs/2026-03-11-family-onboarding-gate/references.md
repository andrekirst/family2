# References for Mandatory Family Onboarding Gate

## Similar Implementations

### familyMemberGuard

- **Location:** `src/frontend/family-hub-web/src/app/core/auth/auth.guard.ts`
- **Relevance:** Existing guard that redirects users without a family. Updated to redirect to `/family/onboarding` instead of `/family`.
- **Key patterns:** `userService.whenReady()` for async user data, `router.parseUrl()` for redirects

### InvitationAcceptComponent

- **Location:** `src/frontend/family-hub-web/src/app/features/family/components/invitation-accept/invitation-accept.component.ts`
- **Relevance:** Full-screen layout pattern with accept/decline invitation flow
- **Key patterns:** `min-h-screen flex items-center justify-center bg-gray-50` layout, signal-based state, `userService.fetchCurrentUser()` after accept, `router.navigate(['/dashboard'])` on success

### CreateFamilyDialogComponent

- **Location:** `src/frontend/family-hub-web/src/app/features/family/components/create-family-dialog/create-family-dialog.component.ts`
- **Relevance:** Family creation form with name input and validation
- **Key patterns:** `familyName` signal, `isLoading` signal, `FamilyService.createFamily()` call, name validation (`trim()` check)

### InvitationService

- **Location:** `src/frontend/family-hub-web/src/app/features/family/services/invitation.service.ts`
- **Relevance:** All invitation operations already implemented
- **Key methods:** `getMyPendingInvitations()`, `acceptInvitationById()`, `declineInvitationById()`
