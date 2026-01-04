import { inject } from '@angular/core';
import { FamilyService } from '../../features/family/services/family.service';
import { AuthService } from '../services/auth.service';

/**
 * App initializer that preloads family data for authenticated users.
 *
 * This ensures that FamilyService.hasFamily() returns accurate results
 * immediately when route guards execute, preventing race conditions
 * where users might briefly see pages they shouldn't have access to.
 *
 * Flow:
 * 1. Check if user is authenticated
 * 2. If yes, load current family data
 * 3. If no, skip loading (user will be redirected to login by authGuard)
 *
 * @returns Promise that resolves when family data is loaded (or skipped)
 */
export function initializeFamily(): () => Promise<void> {
  const familyService = inject(FamilyService);
  const authService = inject(AuthService);

  return async () => {
    // Only load family data if user is authenticated
    // This prevents unnecessary GraphQL calls for unauthenticated users
    if (!authService.isAuthenticated()) {
      console.log('[FamilyInitializer] User not authenticated, skipping family load');
      return;
    }

    console.log('[FamilyInitializer] Loading family data...');

    try {
      await familyService.loadCurrentFamily();

      if (familyService.hasFamily()) {
        console.log('[FamilyInitializer] Family data loaded successfully');
      } else {
        console.log('[FamilyInitializer] User has no family');
      }
    } catch (error) {
      console.error('[FamilyInitializer] Failed to load family data:', error);
      // Don't throw - allow app to continue even if family load fails
      // Auth guard will handle redirects appropriately
    }
  };
}
