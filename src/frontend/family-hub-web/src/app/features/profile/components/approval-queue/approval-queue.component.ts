import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileChangeRequestService } from '../../services/profile-change-request.service';
import { ProfileChangeRequest, getFieldLabel } from '../../models/profile-change-request.models';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';

/**
 * Approval queue component for Owner/Admin users.
 *
 * Displays pending profile change requests from child users
 * and allows parents to approve or reject changes.
 *
 * Features:
 * - Lists all pending changes grouped by child
 * - Shows field name, old value, new value
 * - Approve button applies the change immediately
 * - Reject button opens a dialog for required reason (min 10 chars)
 */
@Component({
  selector: 'app-approval-queue',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent],
  template: `
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-xl font-semibold text-gray-900">Pending Approvals</h2>
          <p class="text-sm text-gray-500 mt-1">
            Review and approve profile changes requested by family members.
          </p>
        </div>
        @if (service.pendingCount() > 0) {
          <span
            class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-yellow-100 text-yellow-800"
          >
            {{ service.pendingCount() }} pending
          </span>
        }
      </div>

      <!-- Error Message -->
      @if (service.approvalQueueError()) {
        <div class="p-4 bg-red-50 border border-red-200 rounded-lg flex items-start" role="alert">
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
            <p class="text-sm text-red-800">{{ service.approvalQueueError() }}</p>
          </div>
          <button
            type="button"
            class="text-red-600 hover:text-red-800"
            (click)="service.clearApprovalQueueError()"
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

      <!-- Loading State -->
      @if (service.isLoadingPending()) {
        <div class="flex items-center justify-center py-12">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <span class="ml-3 text-gray-600">Loading pending changes...</span>
        </div>
      }

      <!-- Empty State -->
      @if (!service.isLoadingPending() && service.pendingCount() === 0) {
        <div class="text-center py-12 bg-gray-50 rounded-lg">
          <svg
            class="mx-auto h-12 w-12 text-gray-400"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <h3 class="mt-4 text-lg font-medium text-gray-900">All caught up!</h3>
          <p class="mt-2 text-sm text-gray-500">No pending profile changes to review.</p>
        </div>
      }

      <!-- Pending Requests List -->
      @if (!service.isLoadingPending() && service.pendingCount() > 0) {
        <div class="space-y-4">
          @for (request of service.pendingRequests(); track request.id) {
            <div
              class="bg-white border border-gray-200 rounded-lg p-4 shadow-sm hover:shadow-md transition-shadow"
            >
              <!-- Request Header -->
              <div class="flex items-start justify-between mb-3">
                <div>
                  <h3 class="text-base font-medium text-gray-900">
                    {{ request.requestedByDisplayName }}
                  </h3>
                  <p class="text-sm text-gray-500">Requested {{ formatDate(request.createdAt) }}</p>
                </div>
                <span
                  class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                >
                  {{ getFieldLabel(request.fieldName) }}
                </span>
              </div>

              <!-- Change Details -->
              <div class="bg-gray-50 rounded-lg p-3 mb-4">
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <p class="text-xs font-medium text-gray-500 uppercase mb-1">Current Value</p>
                    <p class="text-sm text-gray-700">
                      {{ request.oldValue || '(not set)' }}
                    </p>
                  </div>
                  <div>
                    <p class="text-xs font-medium text-gray-500 uppercase mb-1">Requested Value</p>
                    <p class="text-sm text-gray-900 font-medium">
                      {{ request.newValue }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Action Buttons -->
              <div class="flex items-center justify-end space-x-3">
                <app-button
                  variant="tertiary"
                  size="sm"
                  (clicked)="openRejectDialog(request)"
                  [disabled]="service.isLoadingPending()"
                >
                  <span class="text-red-600">Reject</span>
                </app-button>
                <app-button
                  variant="primary"
                  size="sm"
                  (clicked)="approveChange(request.id)"
                  [disabled]="service.isLoadingPending()"
                >
                  Approve
                </app-button>
              </div>
            </div>
          }
        </div>
      }

      <!-- Reject Dialog (Modal) -->
      @if (showRejectDialog()) {
        <div
          class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
          (click)="closeRejectDialog()"
        >
          <div
            class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4 p-6"
            (click)="$event.stopPropagation()"
            role="dialog"
            aria-labelledby="reject-dialog-title"
            aria-modal="true"
          >
            <h3 id="reject-dialog-title" class="text-lg font-semibold text-gray-900 mb-2">
              Reject Profile Change
            </h3>
            <p class="text-sm text-gray-600 mb-4">
              Please provide a reason for rejecting this change. This will be shown to
              {{ selectedRequest()?.requestedByDisplayName }}.
            </p>

            <!-- Rejection Reason Input -->
            <div class="mb-4">
              <label for="rejectReason" class="block text-sm font-medium text-gray-700 mb-1">
                Reason <span class="text-red-600">*</span>
              </label>
              <textarea
                id="rejectReason"
                [(ngModel)]="rejectReason"
                rows="3"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none"
                placeholder="Explain why this change cannot be approved..."
                [class.border-red-500]="rejectReason.length > 0 && rejectReason.length < 10"
              ></textarea>
              <p
                class="mt-1 text-sm"
                [class]="rejectReason.length < 10 ? 'text-red-600' : 'text-gray-500'"
              >
                {{ rejectReason.length }}/10 characters minimum
              </p>
            </div>

            <!-- Dialog Actions -->
            <div class="flex justify-end space-x-3">
              <app-button variant="tertiary" (clicked)="closeRejectDialog()"> Cancel </app-button>
              <app-button
                variant="primary"
                [disabled]="rejectReason.trim().length < 10"
                (clicked)="confirmReject()"
              >
                <span class="text-white">Reject Change</span>
              </app-button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class ApprovalQueueComponent implements OnInit {
  readonly service = inject(ProfileChangeRequestService);

  /**
   * Whether the reject dialog is open.
   */
  showRejectDialog = signal(false);

  /**
   * Currently selected request for rejection.
   */
  selectedRequest = signal<ProfileChangeRequest | null>(null);

  /**
   * Rejection reason input value.
   */
  rejectReason = '';

  ngOnInit(): void {
    this.service.loadPendingForApproval();
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
   * Approves a profile change request.
   */
  async approveChange(requestId: string): Promise<void> {
    await this.service.approve(requestId);
  }

  /**
   * Opens the rejection dialog for a request.
   */
  openRejectDialog(request: ProfileChangeRequest): void {
    this.selectedRequest.set(request);
    this.rejectReason = '';
    this.showRejectDialog.set(true);
  }

  /**
   * Closes the rejection dialog.
   */
  closeRejectDialog(): void {
    this.showRejectDialog.set(false);
    this.selectedRequest.set(null);
    this.rejectReason = '';
  }

  /**
   * Confirms rejection of the selected request.
   */
  async confirmReject(): Promise<void> {
    const request = this.selectedRequest();
    if (!request || this.rejectReason.trim().length < 10) return;

    const success = await this.service.reject(request.id, this.rejectReason.trim());
    if (success) {
      this.closeRejectDialog();
    }
  }
}
