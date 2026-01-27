import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';

/**
 * Account security tab for managing local authentication settings.
 * Provides password change, session management, and account deletion.
 */
@Component({
  selector: 'app-account-security-tab',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, IconComponent, InputComponent],
  template: `
    <div class="space-y-6">
      <!-- Email Display -->
      <div class="bg-gray-50 rounded-lg p-4">
        <div class="flex items-center">
          <app-icon name="mail" size="md" customClass="text-gray-600 mr-3"></app-icon>
          <div>
            <h3 class="text-sm font-medium text-gray-900">Email Address</h3>
            <p class="text-sm text-gray-600">{{ currentUserEmail() }}</p>
            @if (emailVerified()) {
              <span class="inline-flex items-center text-xs text-green-600 mt-1">
                <svg class="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
                  <path
                    fill-rule="evenodd"
                    d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                    clip-rule="evenodd"
                  />
                </svg>
                Verified
              </span>
            } @else {
              <span class="inline-flex items-center text-xs text-yellow-600 mt-1">
                <svg class="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
                  <path
                    fill-rule="evenodd"
                    d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                    clip-rule="evenodd"
                  />
                </svg>
                Not verified
              </span>
            }
          </div>
        </div>
      </div>

      <!-- Account Security Options -->
      <div class="space-y-4">
        <!-- Change Password Section -->
        <div class="p-4 bg-gray-50 rounded-lg">
          <div class="flex items-center justify-between mb-4">
            <div class="flex items-center space-x-3">
              <app-icon name="key" size="md" customClass="text-gray-600"></app-icon>
              <div>
                <h3 class="font-medium text-gray-900">Password</h3>
                <p class="text-sm text-gray-500">Change your account password</p>
              </div>
            </div>
            @if (!showChangePassword()) {
              <app-button variant="secondary" size="sm" (clicked)="toggleChangePassword()">
                Change
              </app-button>
            }
          </div>

          @if (showChangePassword()) {
            <div class="mt-4 space-y-4 border-t pt-4">
              @if (changePasswordError()) {
                <div class="p-3 bg-red-50 border border-red-200 rounded-md">
                  <p class="text-sm text-red-600">{{ changePasswordError() }}</p>
                </div>
              }

              @if (changePasswordSuccess()) {
                <div class="p-3 bg-green-50 border border-green-200 rounded-md">
                  <p class="text-sm text-green-600">Password changed successfully!</p>
                </div>
              }

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Current Password</label>
                <app-input
                  type="password"
                  [(ngModel)]="currentPassword"
                  placeholder="Enter current password"
                  autocomplete="current-password"
                />
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">New Password</label>
                <app-input
                  type="password"
                  [(ngModel)]="newPassword"
                  placeholder="Enter new password"
                  autocomplete="new-password"
                />
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1"
                  >Confirm New Password</label
                >
                <app-input
                  type="password"
                  [(ngModel)]="confirmPassword"
                  placeholder="Confirm new password"
                  autocomplete="new-password"
                />
              </div>

              <div class="flex space-x-3">
                <app-button
                  variant="primary"
                  size="sm"
                  [loading]="isChangingPassword()"
                  [disabled]="!canChangePassword()"
                  (clicked)="changePassword()"
                >
                  Update Password
                </app-button>
                <app-button variant="secondary" size="sm" (clicked)="toggleChangePassword()">
                  Cancel
                </app-button>
              </div>
            </div>
          }
        </div>

        <!-- Active Sessions (Placeholder) -->
        <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
          <div class="flex items-center space-x-3">
            <app-icon name="desktop" size="md" customClass="text-gray-600"></app-icon>
            <div>
              <h3 class="font-medium text-gray-900">Active Sessions</h3>
              <p class="text-sm text-gray-500">View and manage your active login sessions</p>
            </div>
          </div>
          <span class="text-xs text-gray-400 bg-gray-200 px-2 py-1 rounded">Coming Soon</span>
        </div>

        <!-- Two-Factor Authentication (Placeholder) -->
        <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
          <div class="flex items-center space-x-3">
            <app-icon name="shield" size="md" customClass="text-gray-600"></app-icon>
            <div>
              <h3 class="font-medium text-gray-900">Two-Factor Authentication</h3>
              <p class="text-sm text-gray-500">Add an extra layer of security to your account</p>
            </div>
          </div>
          <span class="text-xs text-gray-400 bg-gray-200 px-2 py-1 rounded">Coming Soon</span>
        </div>
      </div>

      <!-- Info Box -->
      <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div class="flex items-start">
          <app-icon name="info-circle" size="sm" customClass="text-blue-600 mt-0.5 mr-3"></app-icon>
          <div>
            <h4 class="text-sm font-medium text-blue-900">About Account Security</h4>
            <p class="mt-1 text-sm text-blue-700">
              Your account is protected with secure password hashing. We recommend using a unique,
              strong password with at least 12 characters including uppercase, lowercase, numbers,
              and special characters.
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
          <span class="text-xs text-gray-400 bg-gray-200 px-2 py-1 rounded">Coming Soon</span>
        </div>
      </div>
    </div>
  `,
})
export class AccountSecurityTabComponent {
  private authService = inject(AuthService);

  // Password change state
  showChangePassword = signal(false);
  isChangingPassword = signal(false);
  changePasswordError = signal<string | null>(null);
  changePasswordSuccess = signal(false);

  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  /**
   * Gets the current user's email address.
   */
  currentUserEmail(): string {
    return this.authService.currentUser()?.email ?? 'Not available';
  }

  /**
   * Gets whether the user's email is verified.
   */
  emailVerified(): boolean {
    return this.authService.emailVerified();
  }

  /**
   * Toggles the change password form visibility.
   */
  toggleChangePassword(): void {
    this.showChangePassword.update((v) => !v);
    if (!this.showChangePassword()) {
      this.resetPasswordForm();
    }
  }

  /**
   * Checks if the change password form can be submitted.
   */
  canChangePassword(): boolean {
    return (
      this.currentPassword.length > 0 &&
      this.newPassword.length >= 12 &&
      this.newPassword === this.confirmPassword &&
      !this.isChangingPassword()
    );
  }

  /**
   * Submits the password change request.
   */
  async changePassword(): Promise<void> {
    if (!this.canChangePassword()) {
      return;
    }

    try {
      this.isChangingPassword.set(true);
      this.changePasswordError.set(null);
      this.changePasswordSuccess.set(false);

      await this.authService.changePassword(
        this.currentPassword,
        this.newPassword,
        this.confirmPassword
      );

      this.changePasswordSuccess.set(true);
      this.resetPasswordForm();

      // Hide success message after 3 seconds
      setTimeout(() => {
        this.changePasswordSuccess.set(false);
        this.showChangePassword.set(false);
      }, 3000);
    } catch (error) {
      console.error('Password change error:', error);
      const message =
        error instanceof Error ? error.message : 'Failed to change password. Please try again.';
      this.changePasswordError.set(message);
    } finally {
      this.isChangingPassword.set(false);
    }
  }

  /**
   * Resets the password form fields.
   */
  private resetPasswordForm(): void {
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
    this.changePasswordError.set(null);
  }
}
