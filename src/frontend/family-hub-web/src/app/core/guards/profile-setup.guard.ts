import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { ProfileService } from '../../features/profile/services/profile.service';

/**
 * Route guard that requires user to have completed profile setup.
 * Redirects to profile setup wizard if displayName is not set.
 *
 * Use Case: Protect dashboard and other routes that require profile context.
 *
 * Guard Chain: authGuard -> profileSetupGuard -> familyGuard -> route
 *
 * NOTE: Profile data is already loaded by APP_INITIALIZER before routing begins.
 * This guard reads from pre-loaded signal state and does not make async calls.
 */
export const profileSetupGuard: CanActivateFn = () => {
  const profileService = inject(ProfileService);
  const router = inject(Router);

  // Profile data already loaded by APP_INITIALIZER
  if (!profileService.isSetupComplete()) {
    console.log('profileSetupGuard: Profile setup incomplete. Redirecting to setup wizard.');
    return router.createUrlTree(['/profile/setup']);
  }

  return true;
};

/**
 * Route guard that requires user to NOT have completed profile setup.
 * Redirects to dashboard if profile is already set up.
 *
 * Use Case: Prevent returning to setup wizard after completion.
 *
 * NOTE: Profile data is already loaded by APP_INITIALIZER before routing begins.
 */
export const noProfileSetupGuard: CanActivateFn = () => {
  const profileService = inject(ProfileService);
  const router = inject(Router);

  if (profileService.isSetupComplete()) {
    console.log('noProfileSetupGuard: Profile already set up. Redirecting to dashboard.');
    return router.createUrlTree(['/dashboard']);
  }

  return true;
};
