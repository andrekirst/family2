# Shared Services

This directory contains reusable Angular services that provide common functionality across the application.

## WizardService

A generic wizard state management service built with Angular v21 Signals for creating multi-step workflows.

### Overview

WizardService manages wizard navigation, step data, and validation state using Angular's reactive Signals API. It supports:

- Multi-step workflows with dynamic component rendering
- Per-step validation with custom validation functions
- Reactive state management with computed signals
- Isolated state per component instance
- Type-safe step data storage and retrieval

### Key Features

1. **Signal-Based State**: Uses Angular Signals for reactive, fine-grained reactivity
2. **Component-Level Scoping**: Each wizard instance has isolated state
3. **Flexible Validation**: Custom validation functions per step
4. **Type Safety**: Generic types for step data storage/retrieval
5. **Immutable Updates**: All state changes create new Map instances

### Basic Usage

```typescript
import { Component, OnInit, inject } from '@angular/core';
import { WizardService, WizardStepConfig } from '@app/shared/services';

@Component({
  selector: 'app-create-family-wizard',
  templateUrl: './create-family-wizard.component.html',
  providers: [WizardService]  // Component-level provider for isolated state
})
export class CreateFamilyWizardComponent implements OnInit {
  private wizardService = inject(WizardService);

  // Expose signals to template
  currentStep = this.wizardService.currentStep;
  totalSteps = this.wizardService.totalSteps;
  currentStepConfig = this.wizardService.currentStepConfig;
  isFirstStep = this.wizardService.isFirstStep;
  isLastStep = this.wizardService.isLastStep;
  canGoNext = this.wizardService.canGoNext;

  ngOnInit(): void {
    this.wizardService.initialize([
      {
        id: 'basic-info',
        componentType: FamilyBasicInfoComponent,
        title: 'Basic Information',
        validateOnNext: (data) => {
          const stepData = data.get('basic-info') as { name?: string } | undefined;
          if (!stepData?.name || stepData.name.trim().length === 0) {
            return ['Family name is required'];
          }
          if (stepData.name.length > 50) {
            return ['Family name must be 50 characters or less'];
          }
          return null;
        }
      },
      {
        id: 'members',
        componentType: FamilyMembersComponent,
        title: 'Family Members',
        canSkip: true
      },
      {
        id: 'confirmation',
        componentType: FamilyConfirmationComponent,
        title: 'Review & Confirm'
      }
    ]);
  }

  next(): void {
    this.wizardService.nextStep();
  }

  previous(): void {
    this.wizardService.previousStep();
  }

  saveStepData(stepId: string, data: unknown): void {
    this.wizardService.setStepData(stepId, data);
  }

  complete(): void {
    // Gather all step data and submit
    const basicInfo = this.wizardService.getStepData<{ name: string }>('basic-info');
    const members = this.wizardService.getStepData<string[]>('members');

    console.log('Creating family:', { basicInfo, members });
  }
}
```

### Template Example

```html
<div class="wizard-container">
  <!-- Progress indicator -->
  <div class="wizard-progress">
    Step {{ currentStep() + 1 }} of {{ totalSteps() }}
  </div>

  <!-- Step content (dynamic component) -->
  <div class="wizard-content">
    <!-- Dynamically render current step component -->
    <ng-container *ngComponentOutlet="currentStepConfig()?.componentType"></ng-container>
  </div>

  <!-- Navigation buttons -->
  <div class="wizard-navigation">
    <button
      type="button"
      (click)="previous()"
      [disabled]="isFirstStep()"
      class="btn btn-secondary">
      Previous
    </button>

    <button
      *ngIf="!isLastStep()"
      type="button"
      (click)="next()"
      [disabled]="!canGoNext()"
      class="btn btn-primary">
      Next
    </button>

    <button
      *ngIf="isLastStep()"
      type="button"
      (click)="complete()"
      class="btn btn-success">
      Complete
    </button>
  </div>

  <!-- Validation errors -->
  <div *ngIf="currentStepConfig() as config" class="wizard-errors">
    <ul *ngIf="wizardService.hasStepErrors(config.id)">
      <li *ngFor="let error of wizardService.getStepErrors(config.id)">
        {{ error }}
      </li>
    </ul>
  </div>
</div>
```

### Step Component Example

```typescript
import { Component, OnInit, inject } from '@angular/core';
import { WizardService } from '@app/shared/services';

interface BasicInfoData {
  name: string;
  description: string;
}

@Component({
  selector: 'app-family-basic-info',
  templateUrl: './family-basic-info.component.html'
})
export class FamilyBasicInfoComponent implements OnInit {
  private wizardService = inject(WizardService);

  name = '';
  description = '';

  ngOnInit(): void {
    // Load existing data if user navigated back
    const existingData = this.wizardService.getStepData<BasicInfoData>('basic-info');
    if (existingData) {
      this.name = existingData.name;
      this.description = existingData.description;
    }
  }

  onInputChange(): void {
    // Save data as user types
    this.wizardService.setStepData<BasicInfoData>('basic-info', {
      name: this.name,
      description: this.description
    });
  }
}
```

### API Reference

#### Configuration Types

```typescript
interface WizardStepConfig {
  id: string;
  componentType: Type<any>;
  title: string;
  canSkip?: boolean;
  validateOnNext?: (stepData: Map<string, unknown>) => string[] | null;
}
```

#### Public Signals (Read-Only)

- `currentStepIndex: Signal<number>` - Current step index (0-based)
- `stepsConfig: Signal<WizardStepConfig[]>` - Step configuration array
- `stepData: Signal<Map<string, unknown>>` - Step data storage
- `stepErrors: Signal<Map<string, string[]>>` - Validation errors per step

#### Computed Signals

- `currentStep: Computed<number>` - Current step index
- `totalSteps: Computed<number>` - Total number of steps
- `currentStepConfig: Computed<WizardStepConfig | undefined>` - Current step config
- `isFirstStep: Computed<boolean>` - Is current step the first?
- `isLastStep: Computed<boolean>` - Is current step the last?
- `canGoNext: Computed<boolean>` - Can navigate to next step?

#### Methods

##### `initialize(steps: WizardStepConfig[]): void`
Initializes wizard with step configurations. Resets all state.

```typescript
wizardService.initialize([
  { id: 'step1', componentType: Step1Component, title: 'Step 1' },
  { id: 'step2', componentType: Step2Component, title: 'Step 2' }
]);
```

##### `nextStep(): void`
Navigates to next step. Validates current step before navigating.

```typescript
wizardService.nextStep();
```

##### `previousStep(): void`
Navigates to previous step. No validation performed.

```typescript
wizardService.previousStep();
```

##### `goToStep(index: number): void`
Jumps to specific step by index.

```typescript
wizardService.goToStep(2); // Jump to third step
```

##### `setStepData<T>(stepId: string, data: T): void`
Stores data for a specific step.

```typescript
wizardService.setStepData<{ name: string }>('step1', { name: 'John' });
```

##### `getStepData<T>(stepId: string): T | undefined`
Retrieves data for a specific step.

```typescript
const data = wizardService.getStepData<{ name: string }>('step1');
console.log(data?.name);
```

##### `validateStep(stepId: string): boolean`
Validates a step using its validation function. Updates stepErrors signal.

```typescript
const isValid = wizardService.validateStep('step1');
```

##### `setStepErrors(stepId: string, errors: string[]): void`
Manually set validation errors for a step.

```typescript
wizardService.setStepErrors('step1', ['Name is required', 'Email is invalid']);
```

##### `clearStepErrors(stepId: string): void`
Clear validation errors for a step.

```typescript
wizardService.clearStepErrors('step1');
```

##### `hasStepErrors(stepId: string): boolean`
Check if a step has validation errors.

```typescript
if (wizardService.hasStepErrors('step1')) {
  console.log('Step 1 has errors');
}
```

##### `getStepErrors(stepId: string): string[]`
Get validation errors for a step.

```typescript
const errors = wizardService.getStepErrors('step1');
errors.forEach(error => console.error(error));
```

##### `reset(): void`
Resets wizard state (navigation, data, errors). Does not clear steps config.

```typescript
wizardService.reset();
```

### Validation Patterns

#### Simple Required Field Validation

```typescript
{
  id: 'step1',
  componentType: Step1Component,
  title: 'Step 1',
  validateOnNext: (data) => {
    const stepData = data.get('step1') as { name?: string } | undefined;
    return stepData?.name ? null : ['Name is required'];
  }
}
```

#### Multiple Field Validation

```typescript
{
  id: 'registration',
  componentType: RegistrationComponent,
  title: 'Registration',
  validateOnNext: (data) => {
    const stepData = data.get('registration') as {
      email?: string;
      password?: string;
      confirmPassword?: string;
    } | undefined;

    const errors: string[] = [];

    if (!stepData?.email) {
      errors.push('Email is required');
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(stepData.email)) {
      errors.push('Email must be valid');
    }

    if (!stepData?.password) {
      errors.push('Password is required');
    } else if (stepData.password.length < 8) {
      errors.push('Password must be at least 8 characters');
    }

    if (stepData?.password !== stepData?.confirmPassword) {
      errors.push('Passwords must match');
    }

    return errors.length > 0 ? errors : null;
  }
}
```

#### Cross-Step Validation

```typescript
{
  id: 'confirmation',
  componentType: ConfirmationComponent,
  title: 'Confirmation',
  validateOnNext: (data) => {
    const basicInfo = data.get('basic-info') as { name?: string } | undefined;
    const members = data.get('members') as string[] | undefined;

    const errors: string[] = [];

    if (!basicInfo?.name) {
      errors.push('Please complete basic information step');
    }

    if (!members || members.length === 0) {
      errors.push('Please add at least one family member');
    }

    return errors.length > 0 ? errors : null;
  }
}
```

### Best Practices

1. **Always provide at component level**: Use `providers: [WizardService]` in component decorator to ensure isolated state per wizard instance.

2. **Type your step data**: Use generics when calling `setStepData<T>()` and `getStepData<T>()` for type safety.

3. **Validate early**: Use `validateOnNext` to catch errors before navigation instead of relying on manual validation.

4. **Save data reactively**: Update wizard data as user types (in step components) to enable reactive validation via `canGoNext` signal.

5. **Load existing data in step components**: When step component initializes, load existing data from wizard service to support backward navigation.

6. **Use computed signals in templates**: Leverage `canGoNext()`, `isFirstStep()`, etc. for declarative UI logic.

7. **Handle completion separately**: Don't navigate on last step - provide separate "Complete" button that gathers all data and submits.

### Testing

See `wizard.service.spec.ts` for comprehensive test examples covering:

- Initial state verification
- Step initialization and validation
- Navigation (next, previous, goToStep)
- Data storage and retrieval
- Validation flow
- Error handling
- Signal reactivity
- Edge cases (single-step, complex data, multiple instances)

Test coverage: 78/78 tests passing (100%)

### Architecture Notes

- **Component-scoped**: Each wizard instance has isolated state via component-level provider
- **Immutable updates**: All Map operations create new instances for proper signal reactivity
- **Reactive validation**: `canGoNext` computed signal automatically validates based on current data
- **No RxJS**: Pure Signals-based implementation following Angular v21+ best practices
- **Type-safe**: Full TypeScript support with generics for step data

### Migration from RxJS-based Wizards

If migrating from RxJS-based wizard services:

1. Replace `BehaviorSubject` with `signal()`
2. Replace `combineLatest()` with `computed()`
3. Replace `.next()` with `.set()` or `.update()`
4. Remove `.pipe()` and operators
5. Update template from `async` pipe to signal invocation `()`
6. Provide service at component level instead of root

### Future Enhancements

Potential future features (not yet implemented):

- Conditional steps based on `canSkip` property
- Step history tracking
- Async validation support
- Step transition animations
- Progress persistence (localStorage)
- Step-specific loading states
- Accessibility improvements (ARIA attributes)

---

For questions or issues, see the comprehensive test suite in `wizard.service.spec.ts` for usage examples.
