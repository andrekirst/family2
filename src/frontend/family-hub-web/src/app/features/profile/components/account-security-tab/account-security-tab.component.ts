import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../core/services/auth.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';
import { environment } from '../../../../../environments/environment';

/**
 * Account security tab with Zitadel-managed features.
 * Links to external Zitadel account management for password changes, MFA, etc.
 *
 * Note: All security operations are handled by Zitadel to ensure maximum security.
 * This tab provides links to the appropriate Zitadel pages.
 */
@Component({
  selector: 'app-account-security-tab',
  standalone: true,
  imports: [CommonModule, ButtonComponent, IconComponent],
  template: `
    <div class="space-y-6">
      <p class="text-gray-600">
        Your account security is managed through our authentication provider (Zitadel). Click below
        to access your security settings.
      </p>

      <!-- Email Display -->
      <div class="bg-gray-50 rounded-lg p-4">
        <div class="flex items-center">
          <app-icon name="mail" size="md" customClass="text-gray-600 mr-3"></app-icon>
          <div>
            <h3 class="text-sm font-medium text-gray-900">Email Address</h3>
            <p class="text-sm text-gray-600">{{ currentUserEmail() }}</p>
            <p class="text-xs text-gray-500 mt-1">
              Managed by your authentication provider. Changes must be made in Zitadel.
            </p>
          </div>
        </div>
      </div>

      <!-- Account Security Options -->
      <div class="space-y-4">
        <!-- Change Password -->
        <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
          <div class="flex items-center space-x-3">
            <app-icon name="key" size="md" customClass="text-gray-600"></app-icon>
            <div>
              <h3 class="font-medium text-gray-900">Password</h3>
              <p class="text-sm text-gray-500">Change your account password</p>
            </div>
          </div>
          <app-button variant="secondary" size="sm" (clicked)="openZitadelAccountSettings()">
            Manage
          </app-button>
        </div>

        <!-- Two-Factor Authentication -->
        <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
          <div class="flex items-center space-x-3">
            <app-icon name="shield" size="md" customClass="text-gray-600"></app-icon>
            <div>
              <h3 class="font-medium text-gray-900">Two-Factor Authentication</h3>
              <p class="text-sm text-gray-500">Add an extra layer of security to your account</p>
            </div>
          </div>
          <app-button variant="secondary" size="sm" (clicked)="openZitadelAccountSettings()">
            Configure
          </app-button>
        </div>

        <!-- Active Sessions -->
        <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
          <div class="flex items-center space-x-3">
            <app-icon name="desktop" size="md" customClass="text-gray-600"></app-icon>
            <div>
              <h3 class="font-medium text-gray-900">Active Sessions</h3>
              <p class="text-sm text-gray-500">View and manage your active login sessions</p>
            </div>
          </div>
          <app-button variant="secondary" size="sm" (clicked)="openZitadelAccountSettings()">
            View
          </app-button>
        </div>

        <!-- Connected Accounts -->
        <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
          <div class="flex items-center space-x-3">
            <app-icon name="link" size="md" customClass="text-gray-600"></app-icon>
            <div>
              <h3 class="font-medium text-gray-900">Connected Accounts</h3>
              <p class="text-sm text-gray-500">Manage linked social or enterprise accounts</p>
            </div>
          </div>
          <app-button variant="secondary" size="sm" (clicked)="openZitadelAccountSettings()">
            Manage
          </app-button>
        </div>
      </div>

      <!-- Info Box -->
      <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div class="flex items-start">
          <app-icon name="info-circle" size="sm" customClass="text-blue-600 mt-0.5 mr-3"></app-icon>
          <div>
            <h4 class="text-sm font-medium text-blue-900">About Account Security</h4>
            <p class="mt-1 text-sm text-blue-700">
              Family Hub uses Zitadel for secure authentication. All security-related changes are
              made through your Zitadel account settings to ensure maximum security and compliance.
            </p>
          </div>
        </div>
      </div>

      <!-- Danger Zone -->
      <div class="mt-8 pt-6 border-t border-gray-200">
        <h3 class="text-lg font-medium text-red-600 mb-4">Danger Zone</h3>
        <div
          class="flex items-center justify-between p-4 bg-red-50 border border-red-200 rounded-lg"
        >
          <div class="flex items-center space-x-3">
            <app-icon name="trash-2" size="md" customClass="text-red-600"></app-icon>
            <div>
              <h3 class="font-medium text-red-900">Delete Account</h3>
              <p class="text-sm text-red-700">
                Permanently delete your account and all associated data
              </p>
            </div>
          </div>
          <button
            type="button"
            class="px-3 py-1.5 text-sm font-medium bg-red-600 text-white rounded-lg hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
            (click)="openAccountDeletion()"
          >
            Delete Account
          </button>
        </div>
      </div>
    </div>
  `,
})
export class AccountSecurityTabComponent {
  private authService = inject(AuthService);

  /**
   * Gets the current user's email address.
   */
  currentUserEmail(): string {
    return this.authService.currentUser()?.email ?? 'Not available';
  }

  /**
   * Opens Zitadel account settings in new tab.
   */
  openZitadelAccountSettings(): void {
    const url = `${environment.zitadelAuthority}/ui/console/users/me`;
    window.open(url, '_blank', 'noopener,noreferrer');
  }

  /**
   * Opens account deletion page (placeholder - links to Zitadel).
   */
  openAccountDeletion(): void {
    // For now, this links to Zitadel account settings
    // A dedicated deletion flow could be implemented in the future
    const url = `${environment.zitadelAuthority}/ui/console/users/me`;
    window.open(url, '_blank', 'noopener,noreferrer');
  }
}
