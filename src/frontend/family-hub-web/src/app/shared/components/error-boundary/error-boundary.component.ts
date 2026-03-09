import { Component, input, signal } from '@angular/core';

@Component({
  selector: 'app-error-boundary',
  standalone: true,
  template: `
    @if (hasError()) {
      <div
        class="flex flex-col items-center justify-center gap-4 rounded-lg border border-red-200 bg-red-50 p-8 text-center dark:border-red-800 dark:bg-red-950"
      >
        <svg
          class="h-12 w-12 text-red-400"
          fill="none"
          viewBox="0 0 24 24"
          stroke-width="1.5"
          stroke="currentColor"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z"
          />
        </svg>
        <h3 class="text-lg font-semibold text-red-800 dark:text-red-200">
          {{ title() }}
        </h3>
        <p class="max-w-md text-sm text-red-600 dark:text-red-300">
          {{ message() }}
        </p>
        <button
          (click)="retry()"
          class="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700 focus:ring-2 focus:ring-red-500 focus:ring-offset-2 focus:outline-none"
        >
          Retry
        </button>
      </div>
    } @else {
      <ng-content />
    }
  `,
})
export class ErrorBoundaryComponent {
  readonly title = input('Something went wrong');
  readonly message = input('An error occurred while loading this section. Please try again.');

  readonly hasError = signal(false);

  triggerError(): void {
    this.hasError.set(true);
  }

  retry(): void {
    this.hasError.set(false);
  }
}
