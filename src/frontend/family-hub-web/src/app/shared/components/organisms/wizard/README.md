# WizardComponent

Generic wizard organism that orchestrates multi-step form flows with dynamic component rendering, validation, and accessibility features.

## Overview

**Purpose:** Main wizard container that dynamically renders step components based on current state. Integrates progress indication, navigation, and validation into a cohesive user experience.

**Architecture:**

- **Atomic Design Level:** Organism (composes atoms/molecules)
- **State Management:** Signal-based with component-scoped `WizardService`
- **Rendering:** Dynamic component loading via `ViewContainerRef`
- **Animations:** 200ms fade transitions with `prefers-reduced-motion` support

## Features

- Dynamic step component rendering
- Automatic data binding and change tracking
- Built-in validation with error handling
- Responsive progress indicator (linear on desktop, dots on mobile)
- Smooth fade transitions between steps
- Screen reader announcements (WCAG 2.1 AA)
- Keyboard navigation support
- Focus management
- Component-scoped state (no global pollution)

## Basic Usage

```typescript
import { Component } from '@angular/core';
import { WizardComponent } from './shared/components/organisms/wizard/wizard.component';
import { WizardStepConfig } from './shared/services/wizard.service';

@Component({
  selector: 'app-create-family',
  imports: [WizardComponent],
  template: `
    <app-wizard
      title="Create Family"
      [steps]="wizardSteps"
      submitButtonText="Create Family"
      (complete)="onWizardComplete($event)"
    ></app-wizard>
  `
})
export class CreateFamilyComponent {
  wizardSteps: WizardStepConfig[] = [
    {
      id: 'family-name',
      componentType: FamilyNameStepComponent,
      title: 'Family Name'
    },
    {
      id: 'family-members',
      componentType: FamilyMembersStepComponent,
      title: 'Add Family Members'
    }
  ];

  onWizardComplete(data: Map<string, unknown>) {
    const familyData = data.get('family-name') as { name: string };
    const membersData = data.get('family-members') as { members: string[] };

    console.log('Family:', familyData.name);
    console.log('Members:', membersData.members);

    // Submit to API...
  }
}
```

## Step Component Contract

Each step component must follow this contract:

```typescript
@Component({
  selector: 'app-example-step',
  template: `
    <div>
      <h2>Example Step</h2>
      <input
        type="text"
        [(ngModel)]="localData.value"
        (ngModelChange)="onDataChange()"
      />
    </div>
  `
})
export class ExampleStepComponent implements OnInit {
  /**
   * Input: Initial data for this step
   * Provided by WizardComponent when step renders
   */
  @Input() data: { value?: string } = {};

  /**
   * Output: Emits when step data changes
   * WizardComponent subscribes to store data
   */
  @Output() dataChange = new EventEmitter<{ value: string }>();

  // Local mutable copy for two-way binding
  localData = { value: '' };

  ngOnInit() {
    // Initialize local data from input
    this.localData = { ...this.data };
  }

  onDataChange() {
    // Emit changes to wizard
    this.dataChange.emit(this.localData);
  }
}
```

## API Reference

### Inputs

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `title` | `string` | `'Wizard'` | Wizard title displayed in header |
| `steps` | `WizardStepConfig[]` | `[]` | Array of step configurations |
| `submitButtonText` | `string` | `'Complete'` | Text for submit button on final step |

### Outputs

| Output | Type | Description |
|--------|------|-------------|
| `complete` | `EventEmitter<Map<string, unknown>>` | Emitted when wizard completes with all step data |
| `cancel` | `EventEmitter<void>` | Emitted when user cancels (future extensibility) |

### WizardStepConfig

```typescript
interface WizardStepConfig {
  /** Unique identifier for this step */
  id: string;

  /** Angular component class to render */
  componentType: Type<any>;

  /** Human-readable title for screen readers */
  title: string;

  /** Whether step can be skipped (future feature) */
  canSkip?: boolean;

  /** Custom validation function */
  validateOnNext?: (stepData: ReadonlyMap<string, unknown>) => string[] | null;
}
```

## Validation

### Step-Level Validation

Add validation to individual steps:

```typescript
const wizardSteps: WizardStepConfig[] = [
  {
    id: 'contact-info',
    componentType: ContactInfoStepComponent,
    title: 'Contact Information',
    validateOnNext: (stepData) => {
      const data = stepData.get('contact-info') as ContactData;
      const errors: string[] = [];

      if (!data?.email) {
        errors.push('Email is required');
      }

      if (!data?.phone) {
        errors.push('Phone number is required');
      }

      return errors.length > 0 ? errors : null;
    }
  }
];
```

### Displaying Validation Errors

Step components can access validation errors:

```typescript
@Component({
  selector: 'app-contact-step',
  template: `
    <div>
      <input type="email" [(ngModel)]="localData.email" />

      @if (errors.length > 0) {
        <div class="error-messages">
          @for (error of errors; track error) {
            <p class="text-red-600">{{ error }}</p>
          }
        </div>
      }
    </div>
  `
})
export class ContactStepComponent implements OnInit {
  @Input() data: ContactData = {};
  @Output() dataChange = new EventEmitter<ContactData>();

  localData: ContactData = {};
  errors: string[] = [];

  constructor(private wizardService: WizardService) {
    // Subscribe to validation errors
    effect(() => {
      this.errors = Array.from(
        this.wizardService.getStepErrors('contact-info')
      );
    });
  }
}
```

## Advanced Examples

### Multi-Step Form with Conditional Logic

```typescript
const wizardSteps: WizardStepConfig[] = [
  {
    id: 'user-type',
    componentType: UserTypeStepComponent,
    title: 'Select User Type',
    validateOnNext: (data) => {
      const stepData = data.get('user-type') as { type?: string };
      return stepData?.type ? null : ['Please select a user type'];
    }
  },
  {
    id: 'business-details',
    componentType: BusinessDetailsStepComponent,
    title: 'Business Details',
    canSkip: true, // Future feature
    validateOnNext: (data) => {
      // Only validate if user selected "business" type
      const userType = data.get('user-type') as { type?: string };

      if (userType?.type !== 'business') {
        return null; // Skip validation for non-business users
      }

      const businessData = data.get('business-details') as BusinessData;
      return businessData?.companyName
        ? null
        : ['Company name is required for business accounts'];
    }
  }
];
```

### Integration with API

```typescript
onWizardComplete(data: Map<string, unknown>) {
  const familyData = data.get('family-name') as { name: string };
  const membersData = data.get('family-members') as { members: Member[] };

  this.familyService.createFamily({
    name: familyData.name,
    members: membersData.members
  }).subscribe({
    next: (family) => {
      this.router.navigate(['/family', family.id]);
    },
    error: (err) => {
      this.errorService.show('Failed to create family');
    }
  });
}
```

### Custom Submit Button Text

```typescript
<app-wizard
  title="Account Setup"
  [steps]="steps"
  submitButtonText="Finish Setup"
  (complete)="onComplete($event)"
></app-wizard>
```

## Accessibility

### Screen Reader Support

- Announces step changes via `aria-live="polite"`
- Provides step context: "Step 2 of 4: Contact Information"
- Back/Next buttons have descriptive labels

### Keyboard Navigation

- Tab: Navigate between inputs within step
- Enter: Submit current input (browser default)
- Buttons are keyboard accessible

### Focus Management

- First input in new step receives focus automatically
- Focus is maintained during step transitions

### Prefers-Reduced-Motion

Animations automatically respect user preferences:

```css
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

## Responsive Design

### Mobile (< 768px)

- Dot progress indicator
- Compact spacing (`px-4 py-6`)
- Touch-friendly button sizing

### Desktop (≥ 768px)

- Linear progress bar with percentage
- Spacious layout (`px-6 lg:px-8`)
- Standard button sizing

## Styling

### Layout Structure

```
┌─────────────────────────────────────┐
│ Header (bg-white shadow)            │
│  - Title (text-2xl font-bold)       │
│  - ProgressBarComponent             │
├─────────────────────────────────────┤
│ Main Content (flex-1 bg-gray-50)    │
│  - Dynamic Step Component           │
│    (max-w-3xl mx-auto)              │
├─────────────────────────────────────┤
│ Footer (bg-white border-t)          │
│  - Back button (left)               │
│  - Next/Submit button (right)       │
└─────────────────────────────────────┘
```

### Customization

Override styles using host element:

```css
app-wizard {
  --wizard-max-width: 800px;
  --wizard-bg-color: #f9fafb;
}

app-wizard::ng-deep .wizard-header {
  background: linear-gradient(to right, #4f46e5, #7c3aed);
  color: white;
}
```

## Testing

### Unit Tests

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WizardComponent } from './wizard.component';

describe('WizardComponent', () => {
  let component: WizardComponent;
  let fixture: ComponentFixture<WizardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WizardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(WizardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should navigate to next step', () => {
    component.steps = [
      { id: 'step1', componentType: Step1Component, title: 'Step 1' },
      { id: 'step2', componentType: Step2Component, title: 'Step 2' }
    ];
    component.ngOnInit();

    expect(component.wizardService.currentStep()).toBe(0);

    component.onNext();

    expect(component.wizardService.currentStep()).toBe(1);
  });
});
```

### E2E Tests

```typescript
describe('Wizard Flow', () => {
  it('should complete multi-step wizard', () => {
    cy.visit('/create-family');

    // Step 1: Family Name
    cy.get('input[name="familyName"]').type('Smith Family');
    cy.contains('Next').click();

    // Step 2: Members
    cy.contains('Step 2 of 3').should('be.visible');
    cy.get('input[name="memberName"]').type('John Smith');
    cy.contains('Add Member').click();
    cy.contains('Next').click();

    // Step 3: Review
    cy.contains('Smith Family').should('be.visible');
    cy.contains('Create Family').click();

    // Verify redirect
    cy.url().should('include', '/family');
  });
});
```

## Performance

### Bundle Size Impact

- Component: ~4 KB (minified + gzipped)
- Dependencies: ProgressBarComponent, ButtonComponent, WizardService
- Total: ~8 KB (including dependencies)

### Optimization Tips

1. **Lazy Load Step Components**: Use dynamic imports for large steps
2. **OnPush Change Detection**: Use in step components
3. **Virtual Scrolling**: For long step lists (future feature)

## Migration Guide

### From Traditional Multi-Step Form

**Before:**

```typescript
// Manual state management
currentStep = 0;
steps = ['Step 1', 'Step 2', 'Step 3'];
formData = {};

nextStep() {
  if (this.currentStep < this.steps.length - 1) {
    this.currentStep++;
  }
}
```

**After:**

```typescript
// Use WizardComponent
<app-wizard
  [steps]="wizardSteps"
  (complete)="onComplete($event)"
></app-wizard>
```

## Troubleshooting

### Step component not rendering

**Problem:** ViewContainerRef is undefined

**Solution:** Ensure `ngAfterViewInit` is called before rendering:

```typescript
ngAfterViewInit() {
  this.renderCurrentStep();
}
```

### Validation not working

**Problem:** Validation errors not displayed

**Solution:** Step component must subscribe to `wizardService.getStepErrors(stepId)`:

```typescript
constructor(private wizardService: WizardService) {
  effect(() => {
    this.errors = Array.from(this.wizardService.getStepErrors('step-id'));
  });
}
```

### Data not persisting between steps

**Problem:** Step data lost when navigating

**Solution:** Emit data changes via `dataChange` EventEmitter:

```typescript
onDataChange() {
  this.dataChange.emit(this.localData);
}
```

## Browser Support

- Chrome/Edge 90+
- Firefox 88+
- Safari 14+
- Mobile: iOS Safari 14+, Chrome Android 90+

## Related Components

- **ProgressBarComponent**: Step progress indicator
- **ButtonComponent**: Navigation buttons
- **WizardService**: State management service

## License

Part of Family Hub project. See main project LICENSE for details.

## Changelog

### Version 1.0.0 (2026-01-03)

- Initial implementation
- Dynamic step rendering
- Validation support
- Accessibility features
- Responsive design
- Fade animations

---

**Last Updated:** 2026-01-03
**Component Version:** 1.0.0
**Family Hub Version:** Phase 0 (Foundation)
