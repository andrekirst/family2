import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from './auth.service';

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
 * Route guard requiring user to belong to a family
 * TODO: Implement by fetching user family data from GraphQL API
 * For now, just checks authentication
 */
export const familyMemberGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    sessionStorage.setItem('redirect_url', state.url);
    return router.parseUrl('/login');
  }

  // TODO: Query GraphQL for user's family membership
  // For now, allow all authenticated users
  return true;
};
