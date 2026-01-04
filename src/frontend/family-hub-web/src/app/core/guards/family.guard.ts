import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { FamilyService } from '../../features/family/services/family.service';

/**
 * Route guard that requires user to have a family.
 * Redirects to family creation wizard if user has no family.
 *
 * **Use Case:** Protect routes that require family context (dashboard, calendar, tasks, etc.)
 *
 * **Behavior:**
 * - If user has family: Allow navigation (return true)
 * - If user has no family: Redirect to /family/create (return false)
 *
 * **When to Use:**
 * - Dashboard routes
 * - Family-specific feature routes
 * - Any route requiring family context
 *
 * **Example:**
 * ```typescript
 * {
 *   path: 'dashboard',
 *   component: DashboardComponent,
 *   canActivate: [authGuard, familyGuard]
 * }
 * ```
 *
 * **Guard Chain:**
 * Typically used after authGuard to ensure user is authenticated AND has family.
 *
 * @param route - Activated route snapshot
 * @param state - Router state snapshot
 * @returns True if user has family, false otherwise (triggers redirect)
 */
export const familyGuard: CanActivateFn = async (route, state) => {
  const familyService = inject(FamilyService);
  const router = inject(Router);

  // Load current family data if not already loaded
  // This ensures guard has fresh data for routing decision
  await familyService.loadCurrentFamily();

  // Check if user has a family using computed signal
  if (!familyService.hasFamily()) {
    console.log('familyGuard: User has no family. Redirecting to family creation wizard.');
    return router.createUrlTree(['/family/create']);
  }

  return true;
};

/**
 * Route guard that requires user to NOT have a family.
 * Redirects to dashboard if user already has a family.
 *
 * **Use Case:** Protect family creation wizard from users who already have families.
 *
 * **Behavior:**
 * - If user has no family: Allow navigation (return true)
 * - If user already has family: Redirect to /dashboard (return false)
 *
 * **When to Use:**
 * - Family creation wizard route
 * - Onboarding routes that should only appear once
 *
 * **Example:**
 * ```typescript
 * {
 *   path: 'family/create',
 *   component: FamilyWizardPageComponent,
 *   canActivate: [authGuard, noFamilyGuard]
 * }
 * ```
 *
 * **Guard Chain:**
 * Typically used after authGuard to ensure user is authenticated but has NO family.
 *
 * **Business Logic:**
 * Prevents users from creating multiple families (current requirement: one family per user).
 * Future enhancement: Support multiple families by removing this guard.
 *
 * @param route - Activated route snapshot
 * @param state - Router state snapshot
 * @returns True if user has no family, false otherwise (triggers redirect)
 */
export const noFamilyGuard: CanActivateFn = async (route, state) => {
  const familyService = inject(FamilyService);
  const router = inject(Router);

  // Load current family data if not already loaded
  // This ensures guard has fresh data for routing decision
  await familyService.loadCurrentFamily();

  // Check if user already has a family
  if (familyService.hasFamily()) {
    console.log('noFamilyGuard: User already has a family. Redirecting to dashboard.');
    return router.createUrlTree(['/dashboard']);
  }

  return true;
};
