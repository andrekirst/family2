import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleIntegrationService } from '../../services/google-integration.service';

@Component({
  selector: 'app-google-link',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-white rounded-lg border border-gray-200 p-6" data-testid="google-link-container">
      <div class="flex items-center justify-between mb-4">
        <div class="flex items-center gap-3">
          <div
            class="w-10 h-10 bg-white rounded-lg border border-gray-200 flex items-center justify-center"
          >
            <svg class="w-6 h-6" viewBox="0 0 24 24">
              <path
                fill="#4285F4"
                d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 01-2.2 3.32v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.1z"
              />
              <path
                fill="#34A853"
                d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
              />
              <path
                fill="#FBBC05"
                d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
              />
              <path
                fill="#EA4335"
                d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
              />
            </svg>
          </div>
          <div>
            <h3 class="text-sm font-semibold text-gray-900">Google Account</h3>
            @if (googleService.isLinked()) {
              <p class="text-xs text-gray-500" data-testid="google-link-email">
                {{ googleService.primaryAccount()?.googleEmail }}
              </p>
            } @else {
              <p class="text-xs text-gray-500">Not connected</p>
            }
          </div>
        </div>

        <div>
          @if (googleService.isLinked()) {
            <span
              class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800"
              data-testid="google-link-status-connected"
            >
              Connected
            </span>
          } @else {
            <span
              class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-600"
              data-testid="google-link-status-disconnected"
            >
              Disconnected
            </span>
          }
        </div>
      </div>

      @if (googleService.isLinked()) {
        <div class="space-y-3">
          <div class="text-xs text-gray-500">
            <p>
              Status: <span class="font-medium">{{ googleService.primaryAccount()?.status }}</span>
            </p>
            @if (googleService.primaryAccount()?.lastSyncAt) {
              <p>Last sync: {{ googleService.primaryAccount()?.lastSyncAt | date: 'medium' }}</p>
            }
            <p>Connected: {{ googleService.primaryAccount()?.createdAt | date: 'mediumDate' }}</p>
          </div>
          <button
            (click)="googleService.unlinkGoogle()"
            [disabled]="googleService.loading()"
            class="px-4 py-2 text-sm font-medium text-red-600 bg-red-50 rounded-lg hover:bg-red-100 transition-colors disabled:opacity-50"
            data-testid="unlink-google"
          >
            {{ googleService.loading() ? 'Unlinking...' : 'Unlink Google Account' }}
          </button>
        </div>
      } @else {
        <button
          (click)="googleService.linkGoogle()"
          [disabled]="googleService.loading()"
          class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
          data-testid="link-google"
        >
          <svg class="w-4 h-4" viewBox="0 0 24 24">
            <path
              fill="#4285F4"
              d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 01-2.2 3.32v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.1z"
            />
            <path
              fill="#34A853"
              d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
            />
            <path
              fill="#FBBC05"
              d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
            />
            <path
              fill="#EA4335"
              d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
            />
          </svg>
          {{ googleService.loading() ? 'Connecting...' : 'Link Google Account' }}
        </button>
      }

      @if (googleService.error()) {
        <p class="mt-3 text-xs text-red-600" data-testid="google-link-error">
          {{ googleService.error() }}
        </p>
      }
    </div>
  `,
})
export class GoogleLinkComponent implements OnInit {
  readonly googleService = inject(GoogleIntegrationService);

  ngOnInit(): void {
    this.googleService.loadLinkedAccounts();
  }
}
