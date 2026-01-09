import { Injectable, Type, signal, computed, Signal, WritableSignal } from '@angular/core';

/**
 * Configuration for a single wizard step.
 */
export interface WizardStepConfig {
  /**
   * Unique identifier for this step.
   * Used as key for step data storage.
   */
  readonly id: string;

  /**
   * Angular component class to render for this step.
   * Component will be dynamically loaded into wizard container.
   * Note: Type<any> is Angular's standard pattern for dynamic components.
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  readonly componentType: Type<any>;

  /**
   * Human-readable title for this step.
   * Used for screen readers and accessibility.
   */
  readonly title: string;

  /**
   * Whether this step can be skipped.
   * Future feature for conditional wizard flows.
   * @default false
   */
  readonly canSkip?: boolean;

  /**
   * Custom validation function for this step.
   * Called before navigating to next step.
   *
   * @param stepData - All wizard step data collected so far (read-only)
   * @returns Array of error messages, or null if validation passes
   */
  readonly validateOnNext?: (stepData: ReadonlyMap<string, unknown>) => string[] | null;
}

/**
 * Generic wizard state management service using Angular Signals.
 * Manages wizard navigation, step data, and validation state.
 *
 * **Scoping:** Should be provided at component level (not root) for isolated state.
 *
 * @example
 * ```typescript
 * @Component({
 *   selector: 'app-wizard',
 *   providers: [WizardService]  // Component-level provider
 * })
 * export class WizardComponent implements OnInit {
 *   private wizardService = inject(WizardService);
 *
 *   ngOnInit() {
 *     this.wizardService.initialize([
 *       {
 *         id: 'step1',
 *         componentType: Step1Component,
 *         title: 'Basic Information',
 *         validateOnNext: (data) => {
 *           const step1Data = data.get('step1') as { name?: string };
 *           return step1Data?.name ? null : ['Name is required'];
 *         }
 *       },
 *       {
 *         id: 'step2',
 *         componentType: Step2Component,
 *         title: 'Additional Details'
 *       }
 *     ]);
 *   }
 *
 *   get currentStep() {
 *     return this.wizardService.currentStep();
 *   }
 *
 *   get totalSteps() {
 *     return this.wizardService.totalSteps();
 *   }
 *
 *   next() {
 *     this.wizardService.nextStep();
 *   }
 * }
 * ```
 */
@Injectable()
export class WizardService {
  // ===== Private Writable Signals =====

  /**
   * Current step index (0-based).
   */
  private readonly _currentStepIndex: WritableSignal<number> = signal(0);

  /**
   * Array of step configurations.
   */
  private readonly _stepsConfig: WritableSignal<WizardStepConfig[]> = signal([]);

  /**
   * Map of step data keyed by step ID.
   * Uses Map for efficient lookups and immutable updates.
   */
  private readonly _stepData: WritableSignal<Map<string, unknown>> = signal(new Map());

  /**
   * Map of validation errors keyed by step ID.
   * Each step can have multiple error messages.
   */
  private readonly _stepErrors: WritableSignal<Map<string, string[]>> = signal(new Map());

  // ===== Public Read-Only Signals =====

  /**
   * Current step index (0-based).
   * Reactive signal that updates when navigation occurs.
   */
  public readonly currentStepIndex: Signal<number> = this._currentStepIndex.asReadonly();

  /**
   * Array of step configurations.
   * Reactive signal that updates when steps are initialized.
   */
  public readonly stepsConfig: Signal<WizardStepConfig[]> = this._stepsConfig.asReadonly();

  /**
   * Map of step data keyed by step ID.
   * Reactive signal that updates when step data is set.
   */
  public readonly stepData: Signal<Map<string, unknown>> = this._stepData.asReadonly();

  /**
   * Map of validation errors keyed by step ID.
   * Reactive signal that updates when validation occurs.
   */
  public readonly stepErrors: Signal<Map<string, string[]>> = this._stepErrors.asReadonly();

  // ===== Computed Signals =====

  /**
   * Current step index (alias for currentStepIndex).
   * Provided for API consistency.
   */
  public readonly currentStep = computed(() => this._currentStepIndex());

  /**
   * Total number of wizard steps.
   * Reactively updates when steps configuration changes.
   */
  public readonly totalSteps = computed(() => this._stepsConfig().length);

  /**
   * Configuration object for current step.
   * Returns undefined if wizard is not initialized or index is invalid.
   */
  public readonly currentStepConfig = computed<WizardStepConfig | undefined>(() => {
    const index = this._currentStepIndex();
    const steps = this._stepsConfig();
    return steps[index];
  });

  /**
   * Whether current step is the first step.
   * Reactively updates when currentStepIndex changes.
   */
  public readonly isFirstStep = computed(() => this._currentStepIndex() === 0);

  /**
   * Whether current step is the last step.
   * Reactively updates when currentStepIndex or stepsConfig changes.
   */
  public readonly isLastStep = computed(() => {
    const index = this._currentStepIndex();
    const total = this._stepsConfig().length;
    return index === total - 1;
  });

  /**
   * Whether navigation to next step is allowed.
   * Returns false if:
   * - Current step is the last step
   * - Current step would fail validation
   *
   * Reactively updates when currentStepIndex, stepsConfig, or stepData change.
   * Note: This performs validation internally to determine if next step is allowed.
   */
  public readonly canGoNext = computed(() => {
    const config = this.currentStepConfig();
    if (!config) {
      return false;
    }

    // Can't go next if on last step
    if (this.isLastStep()) {
      return false;
    }

    // If no validation function, can proceed
    if (!config.validateOnNext) {
      return true;
    }

    // Run validation to check if can proceed
    // Access stepData to create reactive dependency
    const data = this._stepData();
    const errors = config.validateOnNext(data);

    return errors === null || errors.length === 0;
  });

  // ===== Public Methods =====

  /**
   * Initializes wizard with step configurations.
   * Resets all state before setting new configuration.
   *
   * @param steps - Array of step configurations
   * @throws Error if steps array is empty
   *
   * @example
   * ```typescript
   * this.wizardService.initialize([
   *   { id: 'step1', componentType: Step1Component, title: 'Step 1' },
   *   { id: 'step2', componentType: Step2Component, title: 'Step 2' }
   * ]);
   * ```
   */
  public initialize(steps: WizardStepConfig[]): void {
    if (!steps || steps.length === 0) {
      throw new Error('Wizard must have at least one step');
    }

    // Validate unique step IDs
    const stepIds = steps.map((s) => s.id);
    const uniqueIds = new Set(stepIds);
    if (stepIds.length !== uniqueIds.size) {
      throw new Error('Wizard step IDs must be unique');
    }

    this.reset();
    this._stepsConfig.set(steps);
  }

  /**
   * Navigates to the next step.
   * Validates current step before navigating.
   * Does nothing if already on last step or validation fails.
   *
   * @example
   * ```typescript
   * this.wizardService.nextStep();
   * ```
   */
  public nextStep(): void {
    const config = this.currentStepConfig();
    if (!config) {
      return;
    }

    // Don't navigate if on last step
    if (this.isLastStep()) {
      return;
    }

    // Validate current step before navigating
    const isValid = this.validateStep(config.id);
    if (!isValid) {
      return;
    }

    this._currentStepIndex.update((index) => index + 1);
  }

  /**
   * Navigates to the previous step.
   * Does nothing if already on first step.
   *
   * @example
   * ```typescript
   * this.wizardService.previousStep();
   * ```
   */
  public previousStep(): void {
    if (this.isFirstStep()) {
      return;
    }

    this._currentStepIndex.update((index) => index - 1);
  }

  /**
   * Navigates to a specific step by index.
   * Does nothing if index is out of bounds.
   *
   * @param index - Zero-based step index
   *
   * @example
   * ```typescript
   * this.wizardService.goToStep(2); // Jump to third step
   * ```
   */
  public goToStep(index: number): void {
    const total = this._stepsConfig().length;
    if (index < 0 || index >= total) {
      return;
    }

    this._currentStepIndex.set(index);
  }

  /**
   * Stores data for a specific step.
   * Creates new Map instance for immutable update.
   *
   * @param stepId - Unique step identifier
   * @param data - Data to store for this step
   *
   * @example
   * ```typescript
   * this.wizardService.setStepData('step1', { name: 'John Doe', email: 'john@example.com' });
   * ```
   */
  public setStepData<T>(stepId: string, data: T): void {
    this._stepData.update((currentMap) => {
      const newMap = new Map(currentMap);
      newMap.set(stepId, data);
      return newMap;
    });
  }

  /**
   * Retrieves data for a specific step.
   * Returns undefined if step has no data.
   *
   * **Type Safety Note:** Caller is responsible for ensuring type parameter T
   * matches the actual stored data type. No runtime validation is performed.
   * Consider creating type guards for critical data validation.
   *
   * @param stepId - Unique step identifier
   * @returns Step data or undefined
   * @template T - Expected type of the step data (caller's responsibility)
   *
   * @example
   * ```typescript
   * interface Step1Data { name: string; email: string; }
   *
   * // Basic usage (no runtime validation)
   * const step1Data = this.wizardService.getStepData<Step1Data>('step1');
   * console.log(step1Data?.name);
   *
   * // Safe usage with type guard
   * const data = this.wizardService.getStepData<Step1Data>('step1');
   * if (data && isStep1Data(data)) {
   *   console.log(data.name); // Type-safe
   * }
   * ```
   */
  public getStepData<T>(stepId: string): T | undefined {
    return this._stepData().get(stepId) as T | undefined;
  }

  /**
   * Validates a step using its custom validation function.
   * Updates stepErrors signal with validation results.
   *
   * @param stepId - Unique step identifier
   * @returns True if validation passed, false otherwise
   *
   * @example
   * ```typescript
   * const isValid = this.wizardService.validateStep('step1');
   * if (!isValid) {
   *   const errors = this.wizardService.getStepErrors('step1');
   *   console.log('Validation errors:', errors);
   * }
   * ```
   */
  public validateStep(stepId: string): boolean {
    const config = this._stepsConfig().find((s) => s.id === stepId);
    if (!config) {
      return true; // No config = no validation = valid
    }

    // If no validation function, step is valid
    if (!config.validateOnNext) {
      this.clearStepErrors(stepId);
      return true;
    }

    // Run validation function
    const errors = config.validateOnNext(this._stepData());

    if (errors && errors.length > 0) {
      this.setStepErrors(stepId, errors);
      return false;
    }

    this.clearStepErrors(stepId);
    return true;
  }

  /**
   * Sets validation errors for a specific step.
   * Creates new Map instance for immutable update.
   *
   * @param stepId - Unique step identifier
   * @param errors - Array of error messages
   *
   * @example
   * ```typescript
   * this.wizardService.setStepErrors('step1', [
   *   'Name is required',
   *   'Email must be valid'
   * ]);
   * ```
   */
  public setStepErrors(stepId: string, errors: string[]): void {
    this._stepErrors.update((currentMap) => {
      const newMap = new Map(currentMap);
      newMap.set(stepId, errors);
      return newMap;
    });
  }

  /**
   * Clears validation errors for a specific step.
   * Creates new Map instance for immutable update.
   *
   * @param stepId - Unique step identifier
   *
   * @example
   * ```typescript
   * this.wizardService.clearStepErrors('step1');
   * ```
   */
  public clearStepErrors(stepId: string): void {
    this._stepErrors.update((currentMap) => {
      const newMap = new Map(currentMap);
      newMap.delete(stepId);
      return newMap;
    });
  }

  /**
   * Checks if a step has validation errors.
   *
   * @param stepId - Unique step identifier
   * @returns True if step has errors, false otherwise
   *
   * @example
   * ```typescript
   * if (this.wizardService.hasStepErrors('step1')) {
   *   console.log('Step 1 has validation errors');
   * }
   * ```
   */
  public hasStepErrors(stepId: string): boolean {
    const errors = this._stepErrors().get(stepId);
    return errors !== undefined && errors.length > 0;
  }

  /**
   * Gets validation errors for a specific step.
   * Returns empty array if step has no errors.
   *
   * @param stepId - Unique step identifier
   * @returns Readonly array of error messages
   *
   * @example
   * ```typescript
   * const errors = this.wizardService.getStepErrors('step1');
   * errors.forEach(error => console.error(error));
   * ```
   */
  public getStepErrors(stepId: string): readonly string[] {
    return this._stepErrors().get(stepId) ?? [];
  }

  /**
   * Resets wizard state to initial values.
   * Clears all step data, errors, and resets navigation to first step.
   *
   * @example
   * ```typescript
   * this.wizardService.reset();
   * ```
   */
  public reset(): void {
    this._currentStepIndex.set(0);
    this._stepData.set(new Map());
    this._stepErrors.set(new Map());
  }
}
