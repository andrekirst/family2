import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  ViewChild,
  ViewContainerRef,
  ComponentRef,
  AfterViewInit,
  inject,
  ChangeDetectorRef,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { WizardService, WizardStepConfig } from '../../../services/wizard.service';
import { ProgressBarComponent } from '../../atoms/progress-bar/progress-bar.component';
import { ButtonComponent } from '../../atoms/button/button.component';

/**
 * Generic wizard organism that orchestrates multi-step form flows.
 *
 * **Purpose:** Main wizard container that dynamically renders step components
 * based on current state. Integrates progress indication, navigation, and
 * validation into a cohesive user experience.
 *
 * **Architecture Pattern:**
 * - Atomic Design: Organism-level component (composes atoms/molecules)
 * - Dynamic Component Loading: Uses ViewContainerRef for step rendering
 * - Signal-Based State: Leverages WizardService with Angular Signals
 * - Animation Support: 200ms fade transitions with prefers-reduced-motion support
 *
 * **Accessibility Features:**
 * - Screen reader announcements for step changes (aria-live)
 * - Keyboard navigation support (Back/Next buttons)
 * - Focus management when steps change
 * - WCAG 2.1 AA compliant
 *
 * **Responsive Design:**
 * - Mobile-first layout with flexible spacing
 * - Progress bar adapts to viewport (linear on desktop, dots on mobile)
 * - Touch-friendly button sizing
 *
 * @example
 * ```typescript
 * // Define step components (each should have data input and dataChange output)
 * @Component({
 *   selector: 'app-family-name-step',
 *   template: `
 *     <input [(ngModel)]="localData.familyName"
 *            (ngModelChange)="onDataChange()" />
 *   `
 * })
 * export class FamilyNameStepComponent {
 *   @Input() data: { familyName?: string } = {};
 *   @Output() dataChange = new EventEmitter<{ familyName: string }>();
 *
 *   localData = { familyName: '' };
 *
 *   ngOnInit() {
 *     this.localData = { ...this.data };
 *   }
 *
 *   onDataChange() {
 *     this.dataChange.emit(this.localData);
 *   }
 * }
 *
 * // Use wizard in parent component
 * @Component({
 *   template: `
 *     <app-wizard
 *       title="Create Family"
 *       [steps]="wizardSteps"
 *       submitButtonText="Create Family"
 *       (complete)="onWizardComplete($event)"
 *     ></app-wizard>
 *   `
 * })
 * export class CreateFamilyComponent {
 *   wizardSteps: WizardStepConfig[] = [
 *     {
 *       id: 'family-name',
 *       componentType: FamilyNameStepComponent,
 *       title: 'Family Name',
 *       validateOnNext: (data) => {
 *         const stepData = data.get('family-name') as { familyName?: string };
 *         return stepData?.familyName ? null : ['Family name is required'];
 *       }
 *     },
 *     {
 *       id: 'family-members',
 *       componentType: FamilyMembersStepComponent,
 *       title: 'Add Family Members'
 *     }
 *   ];
 *
 *   onWizardComplete(data: Map<string, unknown>) {
 *     const familyData = data.get('family-name') as { familyName: string };
 *     const membersData = data.get('family-members') as { members: string[] };
 *     // Submit to API...
 *   }
 * }
 * ```
 */
@Component({
  selector: 'app-wizard',
  imports: [CommonModule, ProgressBarComponent, ButtonComponent],
  providers: [WizardService], // Component-scoped service for isolated state
  animations: [
    trigger('fadeTransition', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease-in', style({ opacity: 1 })),
      ]),
      transition(':leave', [
        style({ opacity: 1 }),
        animate('200ms ease-out', style({ opacity: 0 })),
      ]),
    ]),
  ],
  template: `
    <div class="min-h-screen bg-gray-50 flex flex-col">
      <!-- Header -->
      <header class="bg-white shadow-sm">
        <div class="max-w-3xl mx-auto px-4 py-6 sm:px-6 lg:px-8">
          <h1 class="text-2xl font-bold text-gray-900 mb-4">{{ title }}</h1>
          <app-progress-bar
            [currentStep]="wizardService.currentStep() + 1"
            [totalSteps]="wizardService.totalSteps()"
            variant="responsive"
          ></app-progress-bar>
        </div>
      </header>

      <!-- Main Content -->
      <main class="flex-1 max-w-3xl mx-auto w-full px-4 py-8 sm:px-6 lg:px-8">
        <div [@fadeTransition]="wizardService.currentStep()">
          <ng-container #stepContainer></ng-container>
        </div>
      </main>

      <!-- Footer -->
      <footer class="bg-white border-t border-gray-200">
        <div class="max-w-3xl mx-auto px-4 py-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <app-button
            variant="tertiary"
            [disabled]="wizardService.isFirstStep()"
            (clicked)="onBack()"
          >
            Back
          </app-button>

          <app-button variant="primary" [disabled]="!canProceed()" (clicked)="onNext()">
            {{ wizardService.isLastStep() ? submitButtonText : 'Next' }}
          </app-button>
        </div>
      </footer>

      <!-- Screen Reader Announcements -->
      <div class="sr-only" role="status" aria-live="polite" aria-atomic="true">
        Step {{ wizardService.currentStep() + 1 }} of {{ wizardService.totalSteps() }}:
        {{ wizardService.currentStepConfig()?.title }}
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
      }

      /* Tailwind's sr-only utility */
      .sr-only {
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        white-space: nowrap;
        border-width: 0;
      }

      /* Respect prefers-reduced-motion */
      @media (prefers-reduced-motion: reduce) {
        * {
          animation-duration: 0.01ms !important;
          transition-duration: 0.01ms !important;
        }
      }
    `,
  ],
})
export class WizardComponent implements OnInit, AfterViewInit, OnDestroy {
  // ===== Inputs =====

  /**
   * Wizard title displayed in header.
   * @default 'Wizard'
   */
  @Input() title = 'Wizard';

  /**
   * Array of step configurations defining the wizard flow.
   * Each step must have a unique ID and component type.
   * @default []
   */
  @Input() steps: WizardStepConfig[] = [];

  /**
   * Text for submit button on final step.
   * @default 'Complete'
   */
  @Input() submitButtonText = 'Complete';

  /**
   * Loading state for submit button.
   * When true, submit button is disabled to prevent duplicate submissions.
   * @default false
   */
  @Input() isSubmitting = false;

  // ===== Outputs =====

  /**
   * Emitted when wizard completes successfully.
   * Contains Map of all step data keyed by step ID.
   *
   * @example
   * ```typescript
   * onComplete(data: Map<string, unknown>) {
   *   const step1Data = data.get('step1') as Step1Data;
   *   const step2Data = data.get('step2') as Step2Data;
   *   // Submit to API...
   * }
   * ```
   */
  @Output() complete = new EventEmitter<Map<string, unknown>>();

  /**
   * Emitted when user cancels the wizard.
   * Note: Current design has no explicit cancel button,
   * but provided for future extensibility.
   */
  @Output() canceled = new EventEmitter<void>();

  // ===== ViewChild References =====

  /**
   * Container for dynamically rendered step components.
   */
  @ViewChild('stepContainer', { read: ViewContainerRef })
  stepContainer!: ViewContainerRef;

  // ===== Services =====

  /**
   * Component-scoped wizard service for state management.
   * Injected via component providers for isolation.
   */
  protected readonly wizardService = inject(WizardService);

  /**
   * Change detector for triggering manual change detection.
   * Required for dynamic component rendering to update view immediately.
   */
  private readonly cdr = inject(ChangeDetectorRef);

  // ===== Private Properties =====

  /**
   * Reference to currently rendered step component.
   * Used for cleanup when navigating between steps.
   */
  private currentStepComponentRef?: ComponentRef<unknown>;

  /**
   * Tracks the last rendered step index to prevent unnecessary re-renders.
   * The effect may trigger due to change detection cycles, not just signal changes.
   */
  private lastRenderedStep = -1;

  // ===== Constructor =====

  constructor() {
    // Watch for currentStep changes and re-render the step component
    // This is critical for wizard navigation to work properly
    effect(() => {
      // Read the currentStep signal to create a reactive dependency
      const step = this.wizardService.currentStep();

      // Guard: Skip rendering if step hasn't actually changed
      // This prevents re-renders triggered by change detection cycles
      if (step === this.lastRenderedStep) {
        return;
      }

      // Only render if:
      // 1. We have a view container (after AfterViewInit)
      // 2. Steps are initialized (prevents running during construction)
      if (this.stepContainer && this.wizardService.totalSteps() > 0) {
        this.lastRenderedStep = step;
        this.renderCurrentStep();
      }
    });
  }

  // ===== Lifecycle Hooks =====

  /**
   * Initializes wizard with provided step configurations.
   */
  ngOnInit(): void {
    if (this.steps.length === 0) {
      throw new Error('Wizard must have at least one step');
    }

    this.wizardService.initialize(this.steps);
  }

  /**
   * Renders initial step after view initialization.
   */
  ngAfterViewInit(): void {
    // Render initial step immediately
    // The effect will handle subsequent step changes
    this.renderCurrentStep();
  }

  /**
   * Cleans up component resources on destruction.
   * Destroys current step component and resets wizard state.
   */
  ngOnDestroy(): void {
    this.cleanupCurrentStep();
    this.wizardService.reset();
  }

  // ===== Public Methods (Navigation) =====

  /**
   * Handles Back button click.
   * Navigates to previous step without validation.
   */
  onBack(): void {
    this.wizardService.previousStep();
  }

  /**
   * Handles Next/Submit button click.
   * Validates current step before navigating or completing wizard.
   */
  onNext(): void {
    const config = this.wizardService.currentStepConfig();
    if (!config) {
      return;
    }

    // Validate current step
    const isValid = this.wizardService.validateStep(config.id);
    if (!isValid) {
      // Validation errors are already set by validateStep()
      // Step component should display them
      return;
    }

    // If last step, emit complete event with all data
    if (this.wizardService.isLastStep()) {
      this.complete.emit(this.wizardService.stepData());
      return;
    }

    // Otherwise, navigate to next step
    this.wizardService.nextStep();
  }

  /**
   * Determines if user can proceed to next step.
   * Checks validation state, last-step logic, and submission state.
   *
   * @returns True if Next/Submit button should be enabled
   */
  canProceed(): boolean {
    const config = this.wizardService.currentStepConfig();
    if (!config) {
      return false;
    }

    // Disable button during submission
    if (this.isSubmitting) {
      return false;
    }

    // If on last step, always allow submit (validation happens on click)
    if (this.wizardService.isLastStep()) {
      return true;
    }

    // For intermediate steps, use canGoNext computed signal
    return this.wizardService.canGoNext();
  }

  // ===== Private Methods (Step Rendering) =====

  /**
   * Dynamically renders the current step component.
   * Cleans up previous step, creates new component instance,
   * and wires up data bindings.
   */
  private renderCurrentStep(): void {
    this.cleanupCurrentStep();

    const config = this.wizardService.currentStepConfig();
    if (!config) {
      return;
    }

    // Create component dynamically
    this.currentStepComponentRef = this.stepContainer.createComponent(config.componentType);

    const instance = this.currentStepComponentRef.instance as Record<string, unknown>;

    // Pass initial data to step component if it has 'data' input
    if ('data' in instance) {
      const stepData = this.wizardService.getStepData(config.id);
      instance['data'] = stepData ?? {};
    }

    // Listen for dataChange events from step component
    if ('dataChange' in instance) {
      const dataChangeEmitter = instance['dataChange'];
      if (
        dataChangeEmitter &&
        typeof (dataChangeEmitter as EventEmitter<unknown>).subscribe === 'function'
      ) {
        (dataChangeEmitter as EventEmitter<unknown>).subscribe((data: unknown) => {
          this.wizardService.setStepData(config.id, data);
          // DON'T call markForCheck here - it triggers the effect() which recreates the component!
        });
      }
    }

    // CRITICAL FIX: Trigger change detection after dynamic component creation
    // This ensures the step component is immediately visible in the DOM
    this.cdr.markForCheck();

    // Focus management: focus first input in new step
    this.focusFirstInput();
  }

  /**
   * Cleans up currently rendered step component.
   * Destroys component instance to prevent memory leaks.
   */
  private cleanupCurrentStep(): void {
    if (this.currentStepComponentRef) {
      this.currentStepComponentRef.destroy();
      this.currentStepComponentRef = undefined;
    }

    // Clear the container
    if (this.stepContainer) {
      this.stepContainer.clear();
    }
  }

  /**
   * Focuses the first input element in the current step.
   * Improves keyboard navigation accessibility.
   *
   * Uses setTimeout to ensure DOM is ready after component render.
   */
  private focusFirstInput(): void {
    setTimeout(() => {
      if (!this.currentStepComponentRef) {
        return;
      }

      // Get native element from component
      const componentElement = (
        this.currentStepComponentRef.location as { nativeElement?: HTMLElement }
      )?.nativeElement;

      if (!componentElement) {
        return;
      }

      // Find first focusable input/textarea/select
      const firstInput = componentElement.querySelector<HTMLElement>(
        'input:not([disabled]), textarea:not([disabled]), select:not([disabled])'
      );

      if (firstInput) {
        firstInput.focus();
      }
    }, 100); // Small delay to ensure DOM is fully rendered
  }
}
