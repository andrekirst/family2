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
 * Redirects to dashboard with error if insufficient permissions
 */
export const familyOwnerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const userProfile = authService.userProfile();
  if (userProfile?.familyRole === 'family-owner') {
    return true;
  }

  return router.parseUrl('/dashboard?error=insufficient_permissions');
};

/**
 * Route guard requiring family admin or owner role
 */
export const familyAdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const userProfile = authService.userProfile();
  const allowedRoles = ['family-owner', 'family-admin'];

  if (userProfile?.familyRole && allowedRoles.includes(userProfile.familyRole)) {
    return true;
  }

  return router.parseUrl('/dashboard?error=insufficient_permissions');
};

/**
 * Route guard requiring user to belong to a family
 * Redirects to family creation if user doesn't have a family
 */
export const familyMemberGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const userProfile = authService.userProfile();
  if (userProfile?.familyId) {
    return true;
  }

  return router.parseUrl('/family/create?error=no_family');
};
