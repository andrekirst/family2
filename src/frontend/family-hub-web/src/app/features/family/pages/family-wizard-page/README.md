# FamilyWizardPageComponent

Page component that orchestrates the family creation wizard using the generic WizardComponent framework.

## Overview

This component provides a guided multi-step flow for creating a new family. Currently implements a single step (family name), but architected for future expansion with additional steps for members, preferences, etc.

## Architecture

- **Pattern:** Page Component (thin wrapper around WizardComponent)
- **Category:** Smart Component (integrates services and routing)
- **Framework:** Uses WizardComponent + WizardService
- **Integration:** FamilyService for API calls, Router for navigation

## Features

### Current Wizard Flow

1. **Step 1:** Family Name (FamilyNameStepComponent)
   - Required field
   - 1-50 characters
   - Whitespace trimmed
   - Validation on navigation

### Future Wizard Flow (Planned)

1. **Step 1:** Family Name - ✅ IMPLEMENTED
2. **Step 2:** Family Members (optional) - PLANNED
3. **Step 3:** Family Preferences (optional) - PLANNED

### Route Guards

- **authGuard:** Ensures user is authenticated
- **noFamilyGuard:** Prevents access if user already has family

### Error Handling

- **Validation Errors:** Displayed in wizard UI, prevent navigation
- **API Errors:** Displayed via FamilyService.error signal
- **Network Errors:** Handled by GraphQLService
- **Missing Data:** Defensive checks with console logging

## Usage

### Route Configuration

```typescript
import { FamilyWizardPageComponent } from './pages/family-wizard-page/family-wizard-page.component';
import { authGuard } from './core/guards/auth.guard';
import { noFamilyGuard } from './core/guards/family.guard';

const routes: Routes = [
  {
    path: 'family/create',
    component: FamilyWizardPageComponent,
    canActivate: [authGuard, noFamilyGuard],
    title: 'Create Your Family'
  }
];
```

### Navigation

```typescript
// Programmatic navigation
this.router.navigate(['/family/create']);

// Template navigation
<a routerLink="/family/create">Create Family</a>
```

## Inputs/Outputs

This component has no inputs or outputs. It's a route-level component that manages its own state and navigation.

## Wizard Configuration

### Step: family-name

```typescript
{
  id: 'family-name',
  componentType: FamilyNameStepComponent,
  title: 'Family Name',
  validateOnNext: (stepData) => {
    const data = stepData.get('family-name') as FamilyNameStepData | undefined;

    // Required validation
    if (!data?.name) {
      return ['Family name is required.'];
    }

    // Whitespace validation
    const trimmedName = data.name.trim();
    if (trimmedName.length === 0) {
      return ['Family name cannot be only whitespace.'];
    }

    // Length validation
    if (trimmedName.length > 50) {
      return ['Family name must be 50 characters or less.'];
    }

    return null; // Valid
  }
}
```

### Validation Rules

1. **Required:** Family name must be provided
2. **Non-Empty:** After trimming, name must have content
3. **Max Length:** 50 characters after trimming
4. **Whitespace:** Leading/trailing whitespace is trimmed before submission

## Component Lifecycle

### 1. Initialization (ngOnInit)

```typescript
ngOnInit() {
  // Guard check: redirect if user already has family
  if (this.familyService.hasFamily()) {
    this.router.navigate(['/dashboard']);
  }
}
```

### 2. User Interaction

- User fills out wizard steps
- Data automatically persisted by WizardService
- Validation on navigation between steps

### 3. Completion (onWizardComplete)

```typescript
async onWizardComplete(event: Map<string, unknown>) {
  // 1. Extract and validate data
  const familyNameData = event.get('family-name') as FamilyNameStepData;

  // 2. Trim whitespace
  const trimmedName = familyNameData.name.trim();

  // 3. Create family via API
  await this.familyService.createFamily(trimmedName);

  // 4. Check for errors
  if (this.familyService.error()) {
    return; // Error displayed in UI
  }

  // 5. Navigate to dashboard
  this.router.navigate(['/dashboard']);
}
```

## Dependencies

### Services

- **FamilyService:** Family state management and API calls
- **Router:** Navigation after wizard completion

### Components

- **WizardComponent:** Generic wizard container
- **FamilyNameStepComponent:** Step 1 component

### Guards

- **authGuard:** Ensures user authentication
- **noFamilyGuard:** Prevents duplicate family creation

## Error Scenarios

### 1. User Already Has Family

**Trigger:** User navigates to /family/create but has existing family
**Behavior:** Redirected to /dashboard by noFamilyGuard or ngOnInit
**User Feedback:** Silent redirect

### 2. Validation Error

**Trigger:** User clicks Next with invalid data
**Behavior:** Wizard displays validation errors, prevents navigation
**User Feedback:** Error messages below fields

### 3. API Error (User Already Has Family)

**Trigger:** User submits wizard, but API returns error
**Behavior:** FamilyService.error signal set, displayed in wizard UI
**User Feedback:** Error message: "User already has a family"

### 4. Network Error

**Trigger:** Network failure during API call
**Behavior:** GraphQLService throws error, caught by FamilyService
**User Feedback:** Error message: "Failed to create family"

### 5. Missing Data (Defensive)

**Trigger:** Wizard completes without required data (shouldn't happen)
**Behavior:** Console error logged, stay on wizard
**User Feedback:** No visual feedback (shouldn't occur)

## Testing

### Unit Tests

```bash
npm test -- family-wizard-page.component.spec.ts
```

### Coverage Areas

- Wizard configuration
- Step validation functions
- ngOnInit guard check
- onWizardComplete success flow
- onWizardComplete error handling
- Template rendering

### Example Test

```typescript
it('should create family and navigate on completion', async () => {
  const stepData = new Map<string, unknown>();
  stepData.set('family-name', { name: '  Smith Family  ' } as FamilyNameStepData);

  mockFamilyService.createFamily.and.returnValue(Promise.resolve());
  mockFamilyService.error.set(null);

  await component.onWizardComplete(stepData);

  expect(mockFamilyService.createFamily).toHaveBeenCalledWith('Smith Family');
  expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
});
```

## Future Enhancements

### Phase 2: Additional Steps

#### Family Members Step

```typescript
{
  id: 'family-members',
  componentType: FamilyMembersStepComponent,
  title: 'Add Family Members',
  canSkip: true,
  validateOnNext: (stepData) => {
    // Optional step - no required validation
    return null;
  }
}
```

#### Family Preferences Step

```typescript
{
  id: 'family-preferences',
  componentType: FamilyPreferencesStepComponent,
  title: 'Family Preferences',
  canSkip: true,
  validateOnNext: (stepData) => {
    // Optional step - no required validation
    return null;
  }
}
```

### Planned Features

- [ ] Skip optional steps
- [ ] Save progress and resume later
- [ ] Invite family members during creation
- [ ] Set family avatar/icon
- [ ] Configure initial preferences
- [ ] Email invitations
- [ ] Social media sharing

## Best Practices

### Do's

- Keep wizard configuration declarative
- Validate data in `validateOnNext` functions
- Trim user input before API submission
- Handle all error scenarios gracefully
- Provide clear user feedback
- Use route guards for access control

### Don'ts

- Don't manipulate WizardService directly (use inputs/outputs)
- Don't skip validation for "convenience"
- Don't navigate without checking for errors
- Don't create families for authenticated users without guards
- Don't display technical error messages to users

## Integration Points

### 1. FamilyService

```typescript
// Create family
await this.familyService.createFamily(name);

// Check for errors
if (this.familyService.error()) {
  // Handle error
}

// Check family status
if (this.familyService.hasFamily()) {
  // Navigate to dashboard
}
```

### 2. WizardComponent

```typescript
<app-wizard
  title="Create Your Family"
  [steps]="wizardSteps"
  submitButtonText="Create Family"
  (complete)="onWizardComplete($event)"
></app-wizard>
```

### 3. Router

```typescript
// Navigate to dashboard
this.router.navigate(['/dashboard']);

// Navigate to wizard
this.router.navigate(['/family/create']);
```

## Migration Notes

### From Modal to Wizard

#### Before (CreateFamilyModalComponent)

```typescript
<app-create-family-modal
  [isOpen]="!familyService.hasFamily()"
  (success)="onFamilyCreated()"
></app-create-family-modal>
```

#### After (FamilyWizardPageComponent)

```typescript
// Route-based wizard
{
  path: 'family/create',
  component: FamilyWizardPageComponent,
  canActivate: [authGuard, noFamilyGuard]
}
```

### Key Changes

1. **Modal → Full Page:** Better UX for multi-step flow
2. **Imperative → Declarative:** Route guards handle access control
3. **Single Step → Multi-Step:** Architected for expansion
4. **Inline Form → Wizard Steps:** Modular step components
5. **Success Event → Navigation:** Router-based flow

## Related Files

- **Component:** `family-wizard-page.component.ts`
- **Test:** `family-wizard-page.component.spec.ts`
- **Step 1:** `components/family-name-step/family-name-step.component.ts`
- **Service:** `services/family.service.ts`
- **Guards:** `core/guards/family.guard.ts`
- **Wizard:** `shared/components/organisms/wizard/wizard.component.ts`

## Support

For questions or issues:

1. Check WizardComponent documentation
2. Review FamilyService for API integration
3. See FamilyNameStepComponent for step implementation
4. Refer to Route Guards for access control patterns

---

**Last Updated:** 2026-01-03
**Version:** 1.0.0
**Status:** Production Ready
