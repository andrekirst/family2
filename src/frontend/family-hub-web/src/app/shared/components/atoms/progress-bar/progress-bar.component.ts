import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-progress-bar',
  imports: [CommonModule],
  template: `
    <!-- Responsive variant: Linear on desktop (md+), Dots on mobile -->
    @if (variant === 'responsive') {
      <!-- Desktop: Linear progress bar (hidden on mobile) -->
      <div
        class="hidden md:block"
        role="progressbar"
        [attr.aria-valuenow]="currentStep"
        [attr.aria-valuemin]="1"
        [attr.aria-valuemax]="totalSteps"
        [attr.aria-label]="ariaLabel"
      >
        <div class="text-center mb-2">
          <span class="text-sm text-gray-600">{{ stepText }}</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
          <div
            class="bg-blue-600 h-2 rounded-full transition-all duration-300 ease-out motion-reduce:transition-none"
            [style.width.%]="progressPercentage"
          ></div>
        </div>
      </div>

      <!-- Mobile: Dot stepper (hidden on desktop) -->
      <div
        class="flex md:hidden justify-center items-center space-x-2"
        role="progressbar"
        [attr.aria-valuenow]="currentStep"
        [attr.aria-valuemin]="1"
        [attr.aria-valuemax]="totalSteps"
        [attr.aria-label]="ariaLabel"
      >
        @for (step of steps; track step) {
          <div
            [class]="getDotClasses(step)"
            [attr.aria-label]="getDotAriaLabel(step)"
            role="presentation"
          ></div>
        }
      </div>
    }

    <!-- Linear variant only -->
    @if (variant === 'linear') {
      <div
        role="progressbar"
        [attr.aria-valuenow]="currentStep"
        [attr.aria-valuemin]="1"
        [attr.aria-valuemax]="totalSteps"
        [attr.aria-label]="ariaLabel"
      >
        <div class="text-center mb-2">
          <span class="text-sm text-gray-600">{{ stepText }}</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
          <div
            class="bg-blue-600 h-2 rounded-full transition-all duration-300 ease-out motion-reduce:transition-none"
            [style.width.%]="progressPercentage"
          ></div>
        </div>
      </div>
    }

    <!-- Dots variant only -->
    @if (variant === 'dots') {
      <div
        class="flex justify-center items-center space-x-2"
        role="progressbar"
        [attr.aria-valuenow]="currentStep"
        [attr.aria-valuemin]="1"
        [attr.aria-valuemax]="totalSteps"
        [attr.aria-label]="ariaLabel"
      >
        @for (step of steps; track step) {
          <div
            [class]="getDotClasses(step)"
            [attr.aria-label]="getDotAriaLabel(step)"
            role="presentation"
          ></div>
        }
      </div>
    }
  `,
  styles: [
    `
      :host {
        display: block;
      }

      /* Respect prefers-reduced-motion */
      @media (prefers-reduced-motion: reduce) {
        * {
          transition-duration: 0.01ms !important;
        }
      }
    `,
  ],
})
export class ProgressBarComponent {
  @Input() currentStep = 1;
  @Input() totalSteps = 1;
  @Input() variant: 'linear' | 'dots' | 'responsive' = 'responsive';

  /**
   * Get progress percentage for linear bar
   * Calculation: ((currentStep - 1) / (totalSteps - 1)) * 100
   * Example: Step 2 of 4 → ((2-1) / (4-1)) * 100 = 33.33%
   */
  get progressPercentage(): number {
    if (this.totalSteps <= 1) return 100;
    return ((this.currentStep - 1) / (this.totalSteps - 1)) * 100;
  }

  /**
   * Get step text for display
   * Example: "Step 2 of 4"
   */
  get stepText(): string {
    return `Step ${this.currentStep} of ${this.totalSteps}`;
  }

  /**
   * Get ARIA label for screen readers
   * Example: "Step 2 of 4"
   */
  get ariaLabel(): string {
    return this.stepText;
  }

  /**
   * Generate array of step numbers for dot iteration
   * Example: totalSteps=4 → [1, 2, 3, 4]
   */
  get steps(): number[] {
    return Array.from({ length: this.totalSteps }, (_, i) => i + 1);
  }

  /**
   * Get Tailwind classes for each dot based on step state
   * Active: blue-600 (current step)
   * Inactive: gray-300 (upcoming step)
   */
  getDotClasses(step: number): string {
    const baseClasses =
      'w-2 h-2 rounded-full transition-colors duration-200 ease-out motion-reduce:transition-none';
    const stateClass = step <= this.currentStep ? 'bg-blue-600' : 'bg-gray-300';
    return `${baseClasses} ${stateClass}`;
  }

  /**
   * Get ARIA label for each dot (screen reader accessibility)
   * Example: "Step 2 completed" or "Step 3 current" or "Step 4 upcoming"
   */
  getDotAriaLabel(step: number): string {
    if (step < this.currentStep) {
      return `Step ${step} completed`;
    } else if (step === this.currentStep) {
      return `Step ${step} current`;
    } else {
      return `Step ${step} upcoming`;
    }
  }
}
