---
name: route-guard
description: Create Angular route guard for authentication/authorization
category: frontend
module-aware: false
inputs:
  - guardName: Guard name (e.g., auth, admin, family-member)
  - guardType: Type of guard (canActivate, canMatch, canDeactivate)
---

# Route Guard Skill

Create functional route guards for authentication and authorization.

## Steps

### 1. Create Auth Guard (Basic)

**Location:** `src/app/guards/auth.guard.ts`

```typescript
import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Store intended URL for redirect after login
  sessionStorage.setItem('redirectUrl', state.url);

  return router.createUrlTree(['/login']);
};
```

### 2. Create Role-Based Guard

**Location:** `src/app/guards/role.guard.ts`

```typescript
import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const userRole = authService.getUserRole();

    if (userRole && allowedRoles.includes(userRole)) {
      return true;
    }

    // Redirect to unauthorized page
    return router.createUrlTree(['/unauthorized']);
  };
};

// Usage in routes:
// { path: 'admin', canActivate: [roleGuard(['ADMIN', 'OWNER'])] }
```

### 3. Create Family Member Guard

**Location:** `src/app/guards/family-member.guard.ts`

```typescript
import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { FamilyService } from '../services/family.service';

export const familyMemberGuard: CanActivateFn = (route, state) => {
  const familyService = inject(FamilyService);
  const router = inject(Router);

  const familyId = route.paramMap.get('familyId');

  if (!familyId) {
    return router.createUrlTree(['/families']);
  }

  return familyService.isMember(familyId).pipe(
    map(isMember => {
      if (isMember) {
        return true;
      }
      return router.createUrlTree(['/unauthorized']);
    }),
    catchError(() => of(router.createUrlTree(['/error'])))
  );
};
```

### 4. Apply Guard to Routes

**Location:** `src/app/app.routes.ts`

```typescript
import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';
import { familyMemberGuard } from './guards/family-member.guard';

export const routes: Routes = [
  // Public routes
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // Protected routes (requires authentication)
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },

  // Role-protected routes
  {
    path: 'admin',
    component: AdminComponent,
    canActivate: [authGuard, roleGuard(['ADMIN', 'OWNER'])]
  },

  // Family-specific routes
  {
    path: 'family/:familyId',
    component: FamilyComponent,
    canActivate: [authGuard, familyMemberGuard]
  },

  // Lazy-loaded protected routes
  {
    path: 'settings',
    loadComponent: () => import('./pages/settings/settings.component')
      .then(m => m.SettingsComponent),
    canActivate: [authGuard]
  }
];
```

## Guard with Data Loading

```typescript
export const familyResolverGuard: CanActivateFn = (route, state) => {
  const familyService = inject(FamilyService);
  const familyId = route.paramMap.get('familyId')!;

  // Load data and make it available via route data
  return familyService.getFamily(familyId).pipe(
    map(family => {
      route.data = { ...route.data, family };
      return true;
    }),
    catchError(() => of(false))
  );
};
```

## CanDeactivate Guard (Unsaved Changes)

```typescript
export interface CanComponentDeactivate {
  canDeactivate: () => boolean | Observable<boolean>;
}

export const unsavedChangesGuard: CanDeactivateFn<CanComponentDeactivate> = (
  component,
  currentRoute,
  currentState,
  nextState
) => {
  if (component.canDeactivate) {
    return component.canDeactivate();
  }
  return true;
};
```

## Validation

- [ ] Guard uses functional syntax (CanActivateFn)
- [ ] Uses inject() for dependency injection
- [ ] Returns boolean, UrlTree, or Observable
- [ ] Applied to routes in app.routes.ts
- [ ] Handles error cases with fallback redirects
