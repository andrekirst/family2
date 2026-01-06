# WizardComponent - Quick Start Guide

**5-Minute Integration Guide**

## 1. Import the Component

```typescript
import { Component } from '@angular/core';
import { WizardComponent } from '@shared/components/organisms/wizard/wizard.component';
import { WizardStepConfig } from '@shared/services/wizard.service';
```

## 2. Create Step Components

Each step needs `@Input() data` and `@Output() dataChange`:

```typescript
@Component({
  selector: 'app-my-step',
  imports: [FormsModule],
  template: `
    <input [(ngModel)]="localData.value" (ngModelChange)="onDataChange()" />
  `
})
export class MyStepComponent {
  @Input() data: { value?: string } = {};
  @Output() dataChange = new EventEmitter<{ value: string }>();

  localData = { value: '' };

  ngOnInit() {
    this.localData = { ...this.data };
  }

  onDataChange() {
    this.dataChange.emit(this.localData);
  }
}
```

## 3. Configure Wizard Steps

```typescript
wizardSteps: WizardStepConfig[] = [
  {
    id: 'step1',
    componentType: Step1Component,
    title: 'Step 1',
    validateOnNext: (data) => {
      const stepData = data.get('step1') as { value?: string };
      return stepData?.value ? null : ['Value is required'];
    }
  },
  {
    id: 'step2',
    componentType: Step2Component,
    title: 'Step 2'
  }
];
```

## 4. Use in Template

```typescript
@Component({
  imports: [WizardComponent],
  template: `
    <app-wizard
      title="My Wizard"
      [steps]="wizardSteps"
      submitButtonText="Complete"
      (complete)="onComplete($event)"
    ></app-wizard>
  `
})
```

## 5. Handle Completion

```typescript
onComplete(data: Map<string, unknown>) {
  const step1Data = data.get('step1') as { value: string };
  const step2Data = data.get('step2') as { name: string };

  // Submit to API or navigate
  console.log('Wizard completed', step1Data, step2Data);
}
```

## That's It

You now have a fully functional wizard with:

- Step navigation (Back/Next)
- Progress indicator
- Validation
- Accessibility
- Smooth animations

## Common Patterns

### Display Validation Errors in Step

```typescript
export class MyStepComponent {
  errors: string[] = [];

  constructor(private wizardService: WizardService) {
    effect(() => {
      this.errors = Array.from(
        this.wizardService.getStepErrors('step-id')
      );
    });
  }
}
```

### Multi-Field Validation

```typescript
validateOnNext: (data) => {
  const stepData = data.get('contact') as ContactData;
  const errors: string[] = [];

  if (!stepData?.email) errors.push('Email required');
  if (!stepData?.phone) errors.push('Phone required');

  return errors.length > 0 ? errors : null;
}
```

### Review Step (Access All Data)

```typescript
@Component({
  template: `
    <div>
      <h2>Review</h2>
      <p>Name: {{ name }}</p>
      <p>Email: {{ email }}</p>
    </div>
  `
})
export class ReviewStepComponent {
  @Input() data: { allData?: Map<string, unknown> } = {};

  name = '';
  email = '';

  ngOnInit() {
    const allData = this.data.allData;
    const step1 = allData?.get('step1') as { name: string };
    const step2 = allData?.get('step2') as { email: string };

    this.name = step1?.name || '';
    this.email = step2?.email || '';
  }
}
```

## Troubleshooting

**Data not persisting?** → Make sure you call `dataChange.emit()` on every change

**Validation not working?** → Return `null` for valid, `string[]` for errors

**Step not rendering?** → Check that component is imported and has correct selector

**Need help?** → See README.md for detailed documentation

---

**Full Documentation:** `README.md`
**Example Usage:** `wizard-example.component.ts`
**API Reference:** Component JSDoc comments
