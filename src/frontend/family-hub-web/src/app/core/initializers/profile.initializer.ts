import { inject } from '@angular/core';
import { ProfileService } from '../../features/profile/services/profile.service';
import { AuthService } from '../services/auth.service';

/**
 * App initializer that preloads profile data for authenticated users.
 *
 * This ensures that ProfileService.isSetupComplete() returns accurate results
 * immediately when route guards execute, preventing race conditions
 * where users might briefly see pages they shouldn't have access to.
 *
 * Flow:
 * 1. Check if user is authenticated
 * 2. If yes, load profile data
 * 3. If no, skip loading (user will be redirected to login by authGuard)
 *
 * NOTE: Runs AFTER family initializer in the provider chain.
 * Profile guard depends on this initializer completing first.
 *
 * @returns Promise that resolves when profile data is loaded (or skipped)
 */
export function initializeProfile(): () => Promise<void> {
  const profileService = inject(ProfileService);
  const authService = inject(AuthService);

  return async () => {
    // Only load profile data if user is authenticated
    // This prevents unnecessary GraphQL calls for unauthenticated users
    if (!authService.isAuthenticated()) {
      console.log('[ProfileInitializer] User not authenticated, skipping profile load');
      return;
    }

    console.log('[ProfileInitializer] Loading profile data...');

    try {
      await profileService.loadProfile();

      if (profileService.isSetupComplete()) {
        console.log('[ProfileInitializer] Profile setup complete');
      } else {
        console.log(
          '[ProfileInitializer] Profile setup incomplete - will redirect to setup wizard'
        );
      }
    } catch (error) {
      console.error('[ProfileInitializer] Failed to load profile data:', error);
      // Don't throw - allow app to continue even if profile load fails
      // Profile guard will handle redirects appropriately
    }
  };
}
