import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MainLayoutComponent } from '../../../../shared/layout/main-layout/main-layout.component';
import { ProfileService } from '../../services/profile.service';
import { ProfileChangeRequestService } from '../../services/profile-change-request.service';
import { PersonalInfoTabComponent } from '../../components/personal-info-tab/personal-info-tab.component';
import { PreferencesTabComponent } from '../../components/preferences-tab/preferences-tab.component';
import { PrivacyTabComponent } from '../../components/privacy-tab/privacy-tab.component';
import { AccountSecurityTabComponent } from '../../components/account-security-tab/account-security-tab.component';
import { ApprovalQueueComponent } from '../../components/approval-queue/approval-queue.component';
import { PendingChangesIndicatorComponent } from '../../components/pending-changes-indicator/pending-changes-indicator.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';

type TabId = 'personal' | 'preferences' | 'privacy' | 'security' | 'approvals';

interface Tab {
  id: TabId;
  label: string;
  icon: string;
}

/**
 * Profile page with tabbed interface.
 * Displays user profile information across 5 tabs:
 * - Personal Info: Display name, birthday, pronouns
 * - Preferences: Language, timezone, date format
 * - Privacy: Field visibility settings
 * - Account Security: Zitadel account links
 * - Approvals: Review pending profile changes from children (Owner/Admin only)
 *
 * Also shows a pending changes indicator at the top for child users
 * who have profile changes awaiting parent approval.
 *
 * @example
 * Route: /profile
 * Guard: authGuard, profileSetupGuard
 */
@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [
    CommonModule,
    MainLayoutComponent,
    PersonalInfoTabComponent,
    PreferencesTabComponent,
    PrivacyTabComponent,
    AccountSecurityTabComponent,
    ApprovalQueueComponent,
    PendingChangesIndicatorComponent,
    IconComponent,
  ],
  template: `
    <app-main-layout>
      <div class="max-w-4xl mx-auto">
        <!-- Page Header -->
        <div class="mb-6">
          <h1 class="text-2xl font-bold text-gray-900">Profile Settings</h1>
          <p class="mt-1 text-gray-600">Manage your personal information and preferences.</p>
        </div>

        <!-- Pending Changes Indicator (for child users) -->
        <app-pending-changes-indicator></app-pending-changes-indicator>

        <!-- Success Message -->
        @if (successMessage()) {
          <div
            class="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg text-green-800"
            role="alert"
            aria-live="polite"
          >
            <div class="flex items-center">
              <app-icon name="check-circle" size="sm" customClass="text-green-600 mr-2"></app-icon>
              <span><strong>Success!</strong> {{ successMessage() }}</span>
            </div>
          </div>
        }

        <!-- Error Message -->
        @if (profileService.error()) {
          <div
            class="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-800"
            role="alert"
            aria-live="polite"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <app-icon name="alert-circle" size="sm" customClass="text-red-600 mr-2"></app-icon>
                <span><strong>Error:</strong> {{ profileService.error() }}</span>
              </div>
              <button
                type="button"
                class="text-red-600 hover:text-red-800"
                (click)="profileService.clearError()"
                aria-label="Dismiss error"
              >
                <app-icon name="x" size="sm"></app-icon>
              </button>
            </div>
          </div>
        }

        <!-- Tab Navigation & Content -->
        <div class="bg-white rounded-lg shadow">
          <!-- Desktop tabs (horizontal) -->
          <nav
            class="hidden md:flex border-b border-gray-200 p-2 space-x-2"
            role="tablist"
            aria-label="Profile settings tabs"
          >
            @for (tab of tabs; track tab.id) {
              <button
                type="button"
                [class]="getTabClasses(tab.id)"
                [attr.aria-selected]="activeTab() === tab.id"
                [attr.aria-controls]="'tabpanel-' + tab.id"
                [id]="'tab-' + tab.id"
                role="tab"
                (click)="selectTab(tab.id)"
              >
                <app-icon [name]="tab.icon" size="sm" customClass="mr-2"></app-icon>
                {{ tab.label }}
                @if (tab.id === 'approvals' && pendingApprovalCount() > 0) {
                  <span
                    class="ml-2 inline-flex items-center justify-center px-2 py-0.5 text-xs font-medium bg-yellow-100 text-yellow-800 rounded-full"
                    aria-label="{{ pendingApprovalCount() }} pending approvals"
                  >
                    {{ pendingApprovalCount() }}
                  </span>
                }
              </button>
            }
          </nav>

          <!-- Mobile tabs (vertical) -->
          <nav
            class="md:hidden border-b border-gray-200 p-2 space-y-1"
            role="tablist"
            aria-label="Profile settings tabs"
          >
            @for (tab of tabs; track tab.id) {
              <button
                type="button"
                [class]="getTabClasses(tab.id) + ' w-full text-left'"
                [attr.aria-selected]="activeTab() === tab.id"
                [attr.aria-controls]="'tabpanel-' + tab.id"
                [id]="'tab-' + tab.id"
                role="tab"
                (click)="selectTab(tab.id)"
              >
                <app-icon [name]="tab.icon" size="sm" customClass="mr-2"></app-icon>
                {{ tab.label }}
                @if (tab.id === 'approvals' && pendingApprovalCount() > 0) {
                  <span
                    class="ml-2 inline-flex items-center justify-center px-2 py-0.5 text-xs font-medium bg-yellow-100 text-yellow-800 rounded-full"
                    aria-label="{{ pendingApprovalCount() }} pending approvals"
                  >
                    {{ pendingApprovalCount() }}
                  </span>
                }
              </button>
            }
          </nav>

          <!-- Tab Content -->
          <div class="p-6">
            @if (profileService.isLoading() && !profileService.hasProfile()) {
              <div class="flex justify-center py-8">
                <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              </div>
            } @else {
              @switch (activeTab()) {
                @case ('personal') {
                  <div id="tabpanel-personal" role="tabpanel" aria-labelledby="tab-personal">
                    <app-personal-info-tab
                      (saveSuccess)="onSaveSuccess($event)"
                    ></app-personal-info-tab>
                  </div>
                }
                @case ('preferences') {
                  <div id="tabpanel-preferences" role="tabpanel" aria-labelledby="tab-preferences">
                    <app-preferences-tab
                      (saveSuccess)="onSaveSuccess($event)"
                    ></app-preferences-tab>
                  </div>
                }
                @case ('privacy') {
                  <div id="tabpanel-privacy" role="tabpanel" aria-labelledby="tab-privacy">
                    <app-privacy-tab (saveSuccess)="onSaveSuccess($event)"></app-privacy-tab>
                  </div>
                }
                @case ('security') {
                  <div id="tabpanel-security" role="tabpanel" aria-labelledby="tab-security">
                    <app-account-security-tab></app-account-security-tab>
                  </div>
                }
                @case ('approvals') {
                  <div id="tabpanel-approvals" role="tabpanel" aria-labelledby="tab-approvals">
                    <app-approval-queue></app-approval-queue>
                  </div>
                }
              }
            }
          </div>
        </div>
      </div>
    </app-main-layout>
  `,
})
export class ProfilePageComponent implements OnInit {
  readonly profileService = inject(ProfileService);
  readonly changeRequestService = inject(ProfileChangeRequestService);

  /**
   * Available tabs configuration.
   * Includes Approvals tab for Owner/Admin users to review child profile changes.
   */
  readonly tabs: Tab[] = [
    { id: 'personal', label: 'Personal Info', icon: 'user' },
    { id: 'preferences', label: 'Preferences', icon: 'settings' },
    { id: 'privacy', label: 'Privacy', icon: 'shield' },
    { id: 'security', label: 'Account Security', icon: 'lock' },
    { id: 'approvals', label: 'Approvals', icon: 'check-circle' },
  ];

  /**
   * Pending approval count for badge display.
   */
  readonly pendingApprovalCount = computed(() => this.changeRequestService.pendingCount());

  /**
   * Currently active tab.
   */
  activeTab = signal<TabId>('personal');

  /**
   * Success message for save operations.
   */
  successMessage = signal<string | null>(null);

  ngOnInit(): void {
    // Ensure profile is loaded (may already be loaded by initializer)
    if (!this.profileService.hasProfile()) {
      this.profileService.loadProfile();
    }
  }

  /**
   * Switches to the specified tab.
   * Clears success and error messages on tab change.
   */
  selectTab(tabId: TabId): void {
    this.activeTab.set(tabId);
    this.successMessage.set(null);
    this.profileService.clearError();
  }

  /**
   * Handles save success from child tab components.
   * Shows success message and auto-clears after 3 seconds.
   */
  onSaveSuccess(message: string): void {
    this.successMessage.set(message);
    // Auto-clear after 3 seconds
    setTimeout(() => this.successMessage.set(null), 3000);
  }

  /**
   * Gets tab CSS classes for active/inactive states.
   */
  getTabClasses(tabId: TabId): string {
    const base =
      'flex items-center px-4 py-2 font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2';

    if (this.activeTab() === tabId) {
      return `${base} bg-blue-100 text-blue-700 focus:ring-blue-500`;
    }

    return `${base} text-gray-600 hover:bg-gray-100 focus:ring-gray-500`;
  }
}
