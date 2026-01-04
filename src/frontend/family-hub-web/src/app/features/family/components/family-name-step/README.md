# FamilyNameStepComponent

Wizard step component for collecting family name during family creation flow.

## Overview

This component is the first (and currently only) step in the family creation wizard. It provides a user-friendly form for entering a family name with real-time validation, character counting, and accessibility features.

## Architecture

- **Pattern:** Wizard Step Component (implements WizardStepComponent contract)
- **Category:** Smart Component (contains form logic and state)
- **Integration:** Used within WizardComponent via dynamic component loading
- **State Management:** Local form state with reactive emission to WizardService

## Features

### Core Functionality
- Reactive form with Angular Validators
- Real-time character counter (0/50)
- Touch-based validation (errors shown only after blur)
- Data persistence across wizard navigation
- Automatic data emission to WizardService

### Validation Rules
1. **Required:** Family name cannot be empty
2. **Max Length:** 50 characters maximum
3. **Touched:** Errors only shown after field interaction

### Character Counter
- **Gray:** 0-40 characters (normal state)
- **Amber:** 41-50 characters (approaching limit)
- **Red:** 51+ characters (blocked by maxlength attribute)

### Accessibility
- `aria-label`: "Family name"
- `aria-required`: true
- `aria-invalid`: Set based on validation state
- `aria-describedby`: Links to error message
- Error messages with `role="alert"`
- Semantic HTML with proper label-input association

## Usage

### In Wizard Configuration

```typescript
import { FamilyNameStepComponent } from './components/family-name-step/family-name-step.component';

const wizardSteps: WizardStepConfig[] = [
  {
    id: 'family-name',
    componentType: FamilyNameStepComponent,
    title: 'Family Name',
    validateOnNext: (stepData) => {
      const data = stepData.get('family-name') as FamilyNameStepData;
      if (!data?.name) return ['Family name is required'];
      if (data.name.length > 50) return ['Family name too long'];
      return null;
    }
  }
];
```

### Standalone Usage (Testing)

```typescript
import { FamilyNameStepComponent } from './family-name-step.component';

@Component({
  template: `
    <app-family-name-step
      [data]="initialData"
      (dataChange)="onDataChange($event)"
    ></app-family-name-step>
  `
})
export class TestComponent {
  initialData = { name: 'Smith Family' };

  onDataChange(data: FamilyNameStepData) {
    console.log('Family name:', data.name);
  }
}
```

## Inputs

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `data` | `FamilyNameStepData \| undefined` | `undefined` | Initial data from WizardService (for state restoration) |

## Outputs

| Event | Type | Description |
|-------|------|-------------|
| `dataChange` | `EventEmitter<FamilyNameStepData>` | Emits on every form value change |

## Data Interface

```typescript
export interface FamilyNameStepData {
  name: string;
}
```

## Form Structure

```typescript
familyForm = new FormGroup({
  name: new FormControl('', [
    Validators.required,
    Validators.maxLength(50)
  ])
});
```

## Validation Messages

| Error Code | Message |
|------------|---------|
| `required` | "Family name is required" |
| `maxlength` | "Family name must be 50 characters or less" |

## Component Lifecycle

1. **ngOnInit:**
   - Initialize form with `data` input if provided
   - Set up reactive effect for data emission
   - Subscribe to form value changes

2. **User Interaction:**
   - User types in input field
   - Form emits value changes
   - Component emits `dataChange` event
   - WizardService persists data

3. **Navigation:**
   - User navigates to next step
   - WizardService validates data
   - If valid, proceeds to next step
   - If invalid, shows validation errors

4. **Return Navigation:**
   - User navigates back to this step
   - Component receives `data` input
   - Form initializes with previous values

## Dependencies

- **InputComponent:** Shared input component with validation
- **IconComponent:** Users icon for header
- **ReactiveFormsModule:** Angular forms
- **CommonModule:** Angular common directives

## Testing

### Unit Tests

```bash
npm test -- family-name-step.component.spec.ts
```

### Coverage Areas
- Form initialization with/without data
- Validation error messages
- Data change emission
- Template rendering
- Accessibility attributes

### Example Test

```typescript
it('should emit dataChange when form value changes', (done) => {
  component.dataChange.subscribe((data: FamilyNameStepData) => {
    expect(data.name).toBe('Test Family');
    done();
  });

  component.familyForm.patchValue({ name: 'Test Family' });
});
```

## Migration from Modal

This component replaces the family name input from `CreateFamilyModalComponent`. Key differences:

### Before (Modal)
```typescript
// Entire form in modal with submit button
<app-modal>
  <form (ngSubmit)="onSubmit()">
    <app-input formControlName="name"></app-input>
    <button type="submit">Create Family</button>
  </form>
</app-modal>
```

### After (Wizard Step)
```typescript
// Just the form fields, no submit button
<div class="space-y-6">
  <form [formGroup]="familyForm">
    <app-input formControlName="name"></app-input>
  </form>
</div>
```

### Changes
1. **No submit button:** WizardComponent handles navigation
2. **Data emission:** Uses Output instead of service call
3. **No loading state:** WizardComponent handles submission
4. **State restoration:** Supports back navigation with data preservation

## Future Enhancements

### Phase 2+
- [ ] Real-time uniqueness check (backend validation)
- [ ] Suggestion list based on common family names
- [ ] Auto-capitalize first letter
- [ ] Emoji picker for family icon
- [ ] Family name templates

### Accessibility Improvements
- [ ] Screen reader announcement for character count milestones
- [ ] High contrast mode support
- [ ] Keyboard shortcuts for common actions

## Best Practices

### Do's
- Keep form logic isolated to this component
- Emit data changes reactively
- Show errors only after touch
- Provide helpful error messages
- Test all validation scenarios

### Don'ts
- Don't call APIs directly (use WizardService)
- Don't handle navigation (WizardComponent does this)
- Don't show loading spinners (parent handles)
- Don't validate on input (wait for blur)

## Related Files

- **Component:** `family-name-step.component.ts`
- **Test:** `family-name-step.component.spec.ts`
- **Parent:** `family-wizard-page.component.ts`
- **Service:** `wizard.service.ts`
- **Input:** `shared/components/atoms/input/input.component.ts`

## Support

For questions or issues:
1. Check WizardService documentation
2. Review FamilyWizardPageComponent integration
3. See WizardComponent for dynamic loading patterns
4. Refer to Angular Reactive Forms guide

---

**Last Updated:** 2026-01-03
**Version:** 1.0.0
**Status:** Production Ready
