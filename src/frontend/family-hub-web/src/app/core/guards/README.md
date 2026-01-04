# Route Guards

Angular route guards for access control and navigation logic in Family Hub.

## Overview

This directory contains functional route guards that protect routes based on authentication and family status. Guards use Angular Signals for reactive state management and the inject() function for dependency injection.

## Guards

### authGuard

Ensures user is authenticated before accessing protected routes.

**File:** `auth.guard.ts`

**Behavior:**
- ✅ Allow: User is authenticated
- ❌ Redirect: User not authenticated → `/login?returnUrl=<current>`

**Usage:**
```typescript
{
  path: 'dashboard',
  component: DashboardComponent,
  canActivate: [authGuard]
}
```

### familyGuard

Ensures user has a family before accessing family-specific routes.

**File:** `family.guard.ts`

**Behavior:**
- ✅ Allow: User has family
- ❌ Redirect: User has no family → `/family/create`

**Usage:**
```typescript
{
  path: 'dashboard',
  component: DashboardComponent,
  canActivate: [authGuard, familyGuard] // Chain guards
}
```

### noFamilyGuard

Prevents users with families from accessing family creation wizard.

**File:** `family.guard.ts`

**Behavior:**
- ✅ Allow: User has no family
- ❌ Redirect: User has family → `/dashboard`

**Usage:**
```typescript
{
  path: 'family/create',
  component: FamilyWizardPageComponent,
  canActivate: [authGuard, noFamilyGuard] // Chain guards
}
```

## Guard Chaining

Guards are executed in order. Use guard chaining for complex access control:

```typescript
// Example: Dashboard requires auth AND family
{
  path: 'dashboard',
  component: DashboardComponent,
  canActivate: [authGuard, familyGuard],
  title: 'Dashboard'
}

// Example: Wizard requires auth BUT NO family
{
  path: 'family/create',
  component: FamilyWizardPageComponent,
  canActivate: [authGuard, noFamilyGuard],
  title: 'Create Your Family'
}
```

## Guard Patterns

### Functional Guards (Modern Angular)

All guards use the functional `CanActivateFn` pattern:

```typescript
export const guardName: CanActivateFn = (route, state) => {
  const service = inject(Service);
  const router = inject(Router);

  if (/* condition */) {
    return true; // Allow navigation
  }

  router.navigate(['/redirect-path']);
  return false; // Block navigation
};
```

### Benefits
- Simpler than class-based guards
- Better tree-shaking
- Easier testing
- Aligned with modern Angular patterns

## Common Scenarios

### 1. New User (No Family)

**Flow:**
```
1. User logs in
2. authGuard: ✅ Pass
3. familyGuard: ❌ Redirect to /family/create
4. noFamilyGuard: ✅ Pass
5. User creates family
6. Redirect to /dashboard
```

**Route Access:**
- ❌ `/dashboard` - Redirects to wizard
- ✅ `/family/create` - Allowed
- ✅ `/login` - Allowed (public)

### 2. Existing User (Has Family)

**Flow:**
```
1. User logs in
2. authGuard: ✅ Pass
3. familyGuard: ✅ Pass
4. User navigates to dashboard
```

**Route Access:**
- ✅ `/dashboard` - Allowed
- ❌ `/family/create` - Redirects to dashboard
- ✅ `/login` - Allowed (public)

### 3. Unauthenticated User

**Flow:**
```
1. User tries to access /dashboard
2. authGuard: ❌ Redirect to /login?returnUrl=/dashboard
```

**Route Access:**
- ❌ `/dashboard` - Redirects to login
- ❌ `/family/create` - Redirects to login
- ✅ `/login` - Allowed (public)

## Testing

### Unit Tests

Each guard has comprehensive unit tests:

```bash
# Test all guards
npm test -- guards

# Test specific guard
npm test -- auth.guard.spec.ts
npm test -- family.guard.spec.ts
```

### Example Test

```typescript
describe('familyGuard', () => {
  it('should allow navigation when user has family', () => {
    mockFamilyService.hasFamily.set(true);

    const result = TestBed.runInInjectionContext(() =>
      familyGuard(mockRoute, mockState)
    );

    expect(result).toBe(true);
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });

  it('should redirect when user has no family', () => {
    mockFamilyService.hasFamily.set(false);

    const result = TestBed.runInInjectionContext(() =>
      familyGuard(mockRoute, mockState)
    );

    expect(result).toBe(false);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/family/create']);
  });
});
```

## Guard Responsibilities

### authGuard
- Check user authentication status
- Store return URL for post-login redirect
- Protect ALL authenticated routes

### familyGuard
- Check user has family
- Redirect to wizard if no family
- Protect family-specific routes

### noFamilyGuard
- Check user has NO family
- Redirect to dashboard if has family
- Protect family creation wizard

## Error Handling

### Missing Services

Guards handle missing services gracefully:

```typescript
// Service not available - fail safe
if (!service) {
  console.error('Service not available');
  return false;
}
```

### Signal Errors

Guards use signals which are null-safe:

```typescript
// Signal always returns a value (never undefined)
if (familyService.hasFamily()) {
  return true;
}
```

### Navigation Errors

Guards log navigation decisions for debugging:

```typescript
console.log('familyGuard: User has no family. Redirecting to wizard.');
```

## Best Practices

### Do's
- Chain guards in logical order (auth first, then feature guards)
- Use descriptive console.log messages for debugging
- Return true/false clearly (no implicit returns)
- Test both allow and deny scenarios
- Use TypeScript strict mode

### Don'ts
- Don't put business logic in guards (use services)
- Don't make HTTP calls in guards (check cached state)
- Don't nest guards (use chaining instead)
- Don't return promises (use synchronous checks)
- Don't mutate state in guards (read-only)

## Future Guards

### Planned Guards

#### roleGuard
```typescript
// Protect admin routes
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  return authService.hasRole('admin');
};
```

#### subscriptionGuard
```typescript
// Protect premium features
export const premiumGuard: CanActivateFn = (route, state) => {
  const subscriptionService = inject(SubscriptionService);
  return subscriptionService.isPremium();
};
```

#### onboardingGuard
```typescript
// Force onboarding completion
export const onboardingGuard: CanActivateFn = (route, state) => {
  const onboardingService = inject(OnboardingService);
  return onboardingService.isComplete();
};
```

## Guard Debugging

### Enable Debug Logging

```typescript
// In guard file
const DEBUG = true;

if (DEBUG) {
  console.log('Guard:', guardName);
  console.log('Route:', route.url);
  console.log('State:', state.url);
  console.log('Result:', result);
}
```

### Router Tracing

```typescript
// In app.config.ts
provideRouter(routes, withDebugTracing())
```

### DevTools

Use Angular DevTools to inspect:
- Route tree
- Guard execution order
- Navigation events
- State changes

## Migration Guide

### Class-Based to Functional Guards

#### Before (Class-Based)
```typescript
@Injectable()
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    }
    this.router.navigate(['/login']);
    return false;
  }
}
```

#### After (Functional)
```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }
  router.navigate(['/login']);
  return false;
};
```

### Changes
1. **No @Injectable():** Function, not class
2. **inject() instead of constructor:** Functional DI
3. **No class methods:** Direct function
4. **Simpler testing:** No class instantiation

## Related Files

- **Guards:** `auth.guard.ts`, `family.guard.ts`
- **Tests:** `auth.guard.spec.ts`, `family.guard.spec.ts`
- **Services:** `core/services/auth.service.ts`, `features/family/services/family.service.ts`
- **Routes:** `app.routes.ts`

## Support

For questions or issues:
1. Check Angular Router documentation
2. Review guard test files for examples
3. See app.routes.ts for usage patterns
4. Refer to service documentation for state management

---

**Last Updated:** 2026-01-03
**Version:** 1.0.0
**Status:** Production Ready
