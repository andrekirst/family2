import { Type } from '@angular/core';

/**
 * Validation result for wizard step validation.
 * - null: Step is valid
 * - string[]: Step has validation errors
 */
export type ValidationResult = string[] | null;

/**
 * Validation function signature for wizard steps.
 * @param stepData - Current step data map
 * @returns Array of error strings if invalid, null if valid
 */
export type ValidationFunction = (stepData: ReadonlyMap<string, unknown>) => ValidationResult;

/**
 * Configuration for a single wizard step.
 */
export interface WizardStepConfig {
  /**
   * Unique identifier for the step.
   */
  readonly id: string;

  /**
   * Display title for the step.
   */
  readonly title: string;

  /**
   * Angular component type to render for this step.
   * Must implement appropriate wizard step interface.
   * Note: Type<any> is Angular's standard pattern for dynamic components.
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  readonly componentType: Type<any>;

  /**
   * Optional description for the step.
   */
  readonly description?: string;

  /**
   * Optional validation function executed when navigating to next step.
   * @returns Array of error strings if invalid, null if valid
   */
  readonly validateOnNext?: ValidationFunction;

  /**
   * Whether this step can be skipped.
   * @default false
   */
  readonly canSkip?: boolean;
}

/**
 * Navigation direction within the wizard.
 */
export enum WizardNavigationDirection {
  Next = 'next',
  Previous = 'previous',
  Jump = 'jump',
}

/**
 * Navigation event emitted by wizard service.
 */
export interface WizardNavigationEvent {
  readonly direction: WizardNavigationDirection;
  readonly fromStepId: string | null;
  readonly toStepId: string;
  readonly timestamp: Date;
}

/**
 * Overall wizard state.
 */
export enum WizardState {
  NotStarted = 'not-started',
  InProgress = 'in-progress',
  Completed = 'completed',
  Cancelled = 'cancelled',
}

/**
 * Complete wizard configuration.
 */
export interface WizardConfig {
  /**
   * Array of wizard steps in order.
   */
  readonly steps: readonly WizardStepConfig[];

  /**
   * Whether wizard allows navigation to previous steps.
   * @default true
   */
  readonly allowBackNavigation?: boolean;

  /**
   * Whether to clear data when wizard is reset.
   * @default true
   */
  readonly clearDataOnReset?: boolean;
}

/**
 * Error thrown when wizard validation fails.
 */
export class WizardValidationError extends Error {
  constructor(
    public readonly stepId: string,
    public readonly errors: readonly string[]
  ) {
    super(`Validation failed for step "${stepId}": ${errors.join(', ')}`);
    this.name = 'WizardValidationError';
  }
}

/**
 * Error thrown when wizard navigation is invalid.
 */
export class WizardNavigationError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'WizardNavigationError';
  }
}
