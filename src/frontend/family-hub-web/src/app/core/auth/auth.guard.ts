import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from './auth.service';
import { UserService } from '../user/user.service';

/**
 * Route guard requiring user authentication
 * Redirects to login if not authenticated
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Store intended URL for post-login redirect
  sessionStorage.setItem('redirect_url', state.url);
  return router.parseUrl('/login');
};

/**
 * Route guard requiring family owner role.
 * Uses permission-based check: only Owners have 'family:delete'.
 * Redirects unauthenticated users to login, insufficient role to dashboard.
 */
export const familyOwnerGuard: CanActivateFn = async (route, state) => {
  const authService = inject(AuthService);
  const userService = inject(UserService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    sessionStorage.setItem('redirect_url', state.url);
    return router.parseUrl('/login');
  }

  const user = await userService.whenReady();
  if (user?.permissions?.includes('family:delete')) {
    return true;
  }

  return router.parseUrl('/dashboard');
};

/**
 * Route guard requiring family admin or owner role.
 * Uses permission-based check: Owners and Admins have 'family:edit'.
 * Redirects unauthenticated users to login, insufficient role to dashboard.
 */
export const familyAdminGuard: CanActivateFn = async (route, state) => {
  const authService = inject(AuthService);
  const userService = inject(UserService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    sessionStorage.setItem('redirect_url', state.url);
    return router.parseUrl('/login');
  }

  const user = await userService.whenReady();
  if (user?.permissions?.includes('family:edit')) {
    return true;
  }

  return router.parseUrl('/dashboard');
};

/**
 * Route guard requiring user to belong to a family.
 * Redirects unauthenticated users to login, and authenticated users
 * without a family to the family creation page.
 *
 * After a page refresh (F5), currentUser is null because it lives in memory.
 * The guard fetches it from the backend before making a routing decision.
 */
export const familyMemberGuard: CanActivateFn = async () => {
  const authService = inject(AuthService);
  const userService = inject(UserService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.parseUrl('/login');
  }

  // Wait for user data — piggybacks on registerUser() if in-flight,
  // or fetches from backend on F5 refresh
  const user = await userService.whenReady();

  if (!user?.familyId) {
    return router.parseUrl('/family');
  }

  return true;
};
