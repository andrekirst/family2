import { Component, inject, signal, OnDestroy } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';

/**
 * Forgot password component for initiating password reset.
 * Supports both email link (web) and 6-digit code (mobile) flows.
 */
@Component({
  selector: 'app-forgot-password',
  imports: [FormsModule, RouterLink, ButtonComponent, InputComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 mb-2">Family Hub</h1>
          <p class="text-gray-600">Reset your password</p>
        </div>

        <div class="bg-white shadow-lg rounded-lg p-8">
          @if (!emailSent()) {
            <h2 class="text-2xl font-semibold text-gray-900 mb-2 text-center">Forgot Password?</h2>
            <p class="text-gray-600 text-center mb-6">
              Enter your email address and we'll send you instructions to reset your password.
            </p>

            @if (errorMessage()) {
              <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
                <p class="text-sm text-red-600">{{ errorMessage() }}</p>
              </div>
            }

            <form (ngSubmit)="requestReset()" class="space-y-6">
              <div>
                <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
                  Email Address
                </label>
                <app-input
                  id="email"
                  type="email"
                  placeholder="Enter your email address"
                  [(ngModel)]="email"
                  name="email"
                  [required]="true"
                  autocomplete="email"
                  class="w-full"
                />
              </div>

              <div class="space-y-3">
                <label class="block text-sm font-medium text-gray-700">
                  How would you like to reset your password?
                </label>
                <div class="space-y-2">
                  <label class="flex items-center">
                    <input
                      type="radio"
                      name="resetMethod"
                      [value]="false"
                      [(ngModel)]="useMobileCode"
                      class="h-4 w-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                    />
                    <span class="ml-2 text-sm text-gray-700"> Send me a reset link via email </span>
                  </label>
                  <label class="flex items-center">
                    <input
                      type="radio"
                      name="resetMethod"
                      [value]="true"
                      [(ngModel)]="useMobileCode"
                      class="h-4 w-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                    />
                    <span class="ml-2 text-sm text-gray-700">
                      Send me a 6-digit code via email
                    </span>
                  </label>
                </div>
              </div>

              <app-button
                type="submit"
                variant="primary"
                size="lg"
                [loading]="isLoading()"
                [disabled]="!email.trim() || isLoading()"
                class="w-full"
              >
                Send Reset Instructions
              </app-button>
            </form>
          } @else {
            <!-- Success State -->
            <div class="text-center">
              <div
                class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4"
              >
                <svg
                  class="h-6 w-6 text-green-600"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M5 13l4 4L19 7"
                  />
                </svg>
              </div>
              <h2 class="text-2xl font-semibold text-gray-900 mb-2">Check Your Email</h2>
              <p class="text-gray-600 mb-6">
                @if (useMobileCode) {
                  We've sent a 6-digit code to <strong>{{ email }}</strong
                  >. Enter the code on the next screen to reset your password.
                } @else {
                  We've sent a password reset link to <strong>{{ email }}</strong
                  >. Click the link in the email to reset your password.
                }
              </p>

              @if (useMobileCode) {
                <app-button
                  variant="primary"
                  size="lg"
                  (click)="navigateToResetWithCode()"
                  class="w-full mb-4"
                >
                  Enter Code
                </app-button>
              }

              <div class="text-sm text-gray-500">
                <p>Didn't receive the email?</p>
                @if (resendCooldown() > 0) {
                  <p class="mt-1">
                    You can resend in
                    <span class="font-medium">{{ resendCooldown() }}</span> seconds
                  </p>
                } @else {
                  <button
                    type="button"
                    (click)="resendEmail()"
                    [disabled]="isLoading()"
                    class="mt-1 text-blue-600 hover:text-blue-500 font-medium"
                  >
                    Resend email
                  </button>
                }
              </div>
            </div>
          }

          <div class="mt-6 text-center">
            <a routerLink="/login" class="text-sm text-blue-600 hover:text-blue-500">
              Back to sign in
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ForgotPasswordComponent implements OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private resendTimer: ReturnType<typeof setInterval> | null = null;

  email = '';
  useMobileCode = false;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  emailSent = signal(false);
  resendCooldown = signal(0);

  constructor() {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  ngOnDestroy(): void {
    this.clearResendTimer();
  }

  async requestReset(): Promise<void> {
    if (!this.email.trim()) {
      this.errorMessage.set('Please enter your email address');
      return;
    }

    try {
      this.isLoading.set(true);
      this.errorMessage.set(null);

      await this.authService.requestPasswordReset(this.email.trim(), this.useMobileCode);

      this.emailSent.set(true);
      this.startResendCooldown();
    } catch (error) {
      console.error('Password reset request error:', error);
      this.isLoading.set(false);

      // Always show a generic message for security (don't reveal if email exists)
      this.emailSent.set(true);
      this.startResendCooldown();
    }
  }

  async resendEmail(): Promise<void> {
    if (this.resendCooldown() > 0) {
      return;
    }

    try {
      this.isLoading.set(true);
      await this.authService.requestPasswordReset(this.email.trim(), this.useMobileCode);
      this.startResendCooldown();
    } catch (error) {
      console.error('Resend email error:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  navigateToResetWithCode(): void {
    // Navigate to reset password with email in query params for code flow
    this.router.navigate(['/reset-password'], {
      queryParams: { email: this.email, method: 'code' },
    });
  }

  private startResendCooldown(): void {
    this.isLoading.set(false);
    this.resendCooldown.set(60);
    this.clearResendTimer();

    this.resendTimer = setInterval(() => {
      const current = this.resendCooldown();
      if (current <= 1) {
        this.resendCooldown.set(0);
        this.clearResendTimer();
      } else {
        this.resendCooldown.set(current - 1);
      }
    }, 1000);
  }

  private clearResendTimer(): void {
    if (this.resendTimer) {
      clearInterval(this.resendTimer);
      this.resendTimer = null;
    }
  }
}
