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
 * Route guard requiring family owner role
 * TODO: Implement by fetching user family data from GraphQL API
 * For now, just checks authentication
 */
export const familyOwnerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    sessionStorage.setItem('redirect_url', state.url);
    return router.parseUrl('/login');
  }

  // TODO: Query GraphQL for user's family role and check if owner
  // For now, allow all authenticated users
  return true;
};

/**
 * Route guard requiring family admin or owner role
 * TODO: Implement by fetching user family data from GraphQL API
 * For now, just checks authentication
 */
export const familyAdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    sessionStorage.setItem('redirect_url', state.url);
    return router.parseUrl('/login');
  }

  // TODO: Query GraphQL for user's family role and check if admin/owner
  // For now, allow all authenticated users
  return true;
};

/**
 * Route guard requiring user to belong to a family.
 * Redirects unauthenticated users to login, and authenticated users
 * without a family to the family creation page.
 */
export const familyMemberGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const userService = inject(UserService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.parseUrl('/login');
  }

  const user = userService.currentUser();
  if (!user?.familyId) {
    return router.parseUrl('/family');
  }

  return true;
};
