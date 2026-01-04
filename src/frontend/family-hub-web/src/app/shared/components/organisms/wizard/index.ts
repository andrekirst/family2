/**
 * WizardComponent - Public API
 *
 * Main organism for multi-step form flows with dynamic component rendering.
 *
 * @example
 * ```typescript
 * import { WizardComponent } from '@shared/components/organisms/wizard';
 * import { WizardStepConfig } from '@shared/services/wizard.service';
 *
 * @Component({
 *   imports: [WizardComponent],
 *   template: `
 *     <app-wizard
 *       [steps]="wizardSteps"
 *       (complete)="onComplete($event)"
 *     ></app-wizard>
 *   `
 * })
 * export class MyComponent {
 *   wizardSteps: WizardStepConfig[] = [...];
 *
 *   onComplete(data: Map<string, unknown>) {
 *     // Handle completion
 *   }
 * }
 * ```
 *
 * @see {@link README.md} for full documentation
 * @see {@link QUICK_START.md} for 5-minute integration guide
 * @see {@link wizard-example.component.ts} for complete example
 */

export { WizardComponent } from './wizard.component';
