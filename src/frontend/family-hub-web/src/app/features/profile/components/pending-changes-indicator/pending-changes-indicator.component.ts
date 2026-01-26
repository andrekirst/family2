import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileChangeRequestService } from '../../services/profile-change-request.service';
import { getFieldLabel } from '../../models/profile-change-request.models';

/**
 * Pending changes indicator for child users.
 *
 * Shows a banner at the top of the profile page when:
 * - There are pending changes awaiting parent approval
 * - There are rejected changes with feedback
 *
 * Features:
 * - Yellow banner for pending changes
 * - Red banner for rejected changes with reasons
 * - Dismiss button for rejected changes
 */
@Component({
  selector: 'app-pending-changes-indicator',
  standalone: true,
  imports: [CommonModule],
  template: `
    <!-- Loading State -->
    @if (service.isLoadingMyChanges()) {
      <div class="animate-pulse bg-gray-100 rounded-lg p-4 mb-4">
        <div class="h-4 bg-gray-200 rounded w-1/3"></div>
      </div>
    }

    <!-- Pending Changes Banner -->
    @if (!service.isLoadingMyChanges() && service.hasPendingChanges()) {
      <div
        class="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-4"
        role="status"
        aria-live="polite"
      >
        <div class="flex items-start">
          <svg
            class="w-5 h-5 text-yellow-600 mr-3 flex-shrink-0 mt-0.5"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path
              fill-rule="evenodd"
              d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
              clip-rule="evenodd"
            />
          </svg>
          <div class="flex-1">
            <h3 class="text-sm font-medium text-yellow-800">Changes Awaiting Approval</h3>
            <div class="mt-2 text-sm text-yellow-700">
              <p class="mb-2">
                The following changes need parent approval before they take effect:
              </p>
              <ul class="list-disc list-inside space-y-1">
                @for (change of service.myPendingChanges(); track change.id) {
                  <li>
                    <span class="font-medium">{{ getFieldLabel(change.fieldName) }}</span>
                    → "{{ change.newValue }}"
                    <span class="text-yellow-600 text-xs ml-1">
                      ({{ formatDate(change.createdAt) }})
                    </span>
                  </li>
                }
              </ul>
            </div>
          </div>
        </div>
      </div>
    }

    <!-- Rejected Changes Banner -->
    @if (!service.isLoadingMyChanges() && service.hasRejectedChanges()) {
      @for (rejected of service.myRejectedChanges(); track rejected.id) {
        <div class="bg-red-50 border border-red-200 rounded-lg p-4 mb-4" role="alert">
          <div class="flex items-start justify-between">
            <div class="flex items-start">
              <svg
                class="w-5 h-5 text-red-600 mr-3 flex-shrink-0 mt-0.5"
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path
                  fill-rule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                  clip-rule="evenodd"
                />
              </svg>
              <div>
                <h3 class="text-sm font-medium text-red-800">
                  Change Rejected: {{ getFieldLabel(rejected.fieldName) }}
                </h3>
                <div class="mt-2 text-sm text-red-700">
                  <p>
                    Your request to change
                    <span class="font-medium">{{ getFieldLabel(rejected.fieldName) }}</span>
                    to "{{ rejected.newValue }}" was not approved.
                  </p>
                  <p class="mt-2 p-2 bg-red-100 rounded">
                    <span class="font-medium">Reason:</span> {{ rejected.rejectionReason }}
                  </p>
                </div>
              </div>
            </div>
            <button
              type="button"
              class="text-red-600 hover:text-red-800 p-1"
              (click)="dismissRejected(rejected.id)"
              aria-label="Dismiss notification"
            >
              <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path
                  fill-rule="evenodd"
                  d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                  clip-rule="evenodd"
                />
              </svg>
            </button>
          </div>
        </div>
      }
    }

    <!-- Error Message -->
    @if (service.myChangesError()) {
      <div
        class="bg-red-50 border border-red-200 rounded-lg p-4 mb-4 flex items-start"
        role="alert"
      >
        <svg
          class="w-5 h-5 text-red-600 mr-3 flex-shrink-0 mt-0.5"
          fill="currentColor"
          viewBox="0 0 20 20"
        >
          <path
            fill-rule="evenodd"
            d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
            clip-rule="evenodd"
          />
        </svg>
        <div class="flex-1">
          <p class="text-sm text-red-800">{{ service.myChangesError() }}</p>
        </div>
        <button
          type="button"
          class="text-red-600 hover:text-red-800"
          (click)="service.clearMyChangesError()"
          aria-label="Dismiss error"
        >
          <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path
              fill-rule="evenodd"
              d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
              clip-rule="evenodd"
            />
          </svg>
        </button>
      </div>
    }
  `,
})
export class PendingChangesIndicatorComponent implements OnInit {
  readonly service = inject(ProfileChangeRequestService);

  ngOnInit(): void {
    this.service.loadMyChanges();
  }

  /**
   * Gets user-friendly label for a field name.
   */
  getFieldLabel(fieldName: string): string {
    return getFieldLabel(fieldName);
  }

  /**
   * Formats a date string for display.
   */
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffHours / 24);

    if (diffHours < 1) {
      return 'just now';
    } else if (diffHours < 24) {
      return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    } else if (diffDays < 7) {
      return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    } else {
      return date.toLocaleDateString(undefined, {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      });
    }
  }

  /**
   * Dismisses a rejected change notification.
   */
  dismissRejected(requestId: string): void {
    this.service.dismissRejectedChange(requestId);
  }
}
