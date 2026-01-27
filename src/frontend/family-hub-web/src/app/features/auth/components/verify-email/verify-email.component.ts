import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { Subject, takeUntil } from 'rxjs';

/**
 * Verify email component for email address verification.
 * Automatically verifies when a token is present in the URL.
 */
@Component({
  selector: 'app-verify-email',
  imports: [RouterLink, ButtonComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 mb-2">Family Hub</h1>
          <p class="text-gray-600">Email Verification</p>
        </div>

        <div class="bg-white shadow-lg rounded-lg p-8">
          @if (isVerifying()) {
            <!-- Verifying State -->
            <div class="text-center">
              <div class="mx-auto flex items-center justify-center h-12 w-12 mb-4">
                <svg
                  class="animate-spin h-8 w-8 text-blue-600"
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                >
                  <circle
                    class="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    stroke-width="4"
                  ></circle>
                  <path
                    class="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  ></path>
                </svg>
              </div>
              <h2 class="text-xl font-semibold text-gray-900 mb-2">Verifying your email...</h2>
              <p class="text-gray-600">Please wait while we verify your email address.</p>
            </div>
          } @else if (verificationSuccess()) {
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
              <h2 class="text-2xl font-semibold text-gray-900 mb-2">Email Verified!</h2>
              <p class="text-gray-600 mb-6">
                Your email address has been successfully verified. You can now access all features
                of Family Hub.
              </p>
              <app-button
                variant="primary"
                size="lg"
                (click)="navigateToDashboard()"
                class="w-full"
              >
                Go to Dashboard
              </app-button>
            </div>
          } @else if (errorMessage()) {
            <!-- Error State -->
            <div class="text-center">
              <div
                class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 mb-4"
              >
                <svg
                  class="h-6 w-6 text-red-600"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </div>
              <h2 class="text-2xl font-semibold text-gray-900 mb-2">Verification Failed</h2>
              <p class="text-gray-600 mb-6">{{ errorMessage() }}</p>

              @if (canResend()) {
                <div class="space-y-4">
                  @if (resendCooldown() > 0) {
                    <p class="text-sm text-gray-500">
                      You can request a new verification email in
                      <span class="font-medium">{{ resendCooldown() }}</span> seconds
                    </p>
                  } @else {
                    <app-button
                      variant="secondary"
                      size="md"
                      [loading]="isResending()"
                      (click)="resendVerification()"
                      class="w-full"
                    >
                      Resend Verification Email
                    </app-button>
                  }
                </div>
              }
            </div>
          } @else {
            <!-- No Token State -->
            <div class="text-center">
              <div
                class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-yellow-100 mb-4"
              >
                <svg
                  class="h-6 w-6 text-yellow-600"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                  />
                </svg>
              </div>
              <h2 class="text-2xl font-semibold text-gray-900 mb-2">Verification Required</h2>
              <p class="text-gray-600 mb-6">
                Please check your email for a verification link. If you haven't received it, you can
                request a new one below.
              </p>

              @if (authService.isAuthenticated()) {
                <div class="space-y-4">
                  @if (resendCooldown() > 0) {
                    <p class="text-sm text-gray-500">
                      You can request a new verification email in
                      <span class="font-medium">{{ resendCooldown() }}</span> seconds
                    </p>
                  } @else {
                    <app-button
                      variant="primary"
                      size="lg"
                      [loading]="isResending()"
                      (click)="resendVerification()"
                      class="w-full"
                    >
                      Send Verification Email
                    </app-button>
                  }

                  @if (resendSuccess()) {
                    <p class="text-sm text-green-600">
                      Verification email sent! Please check your inbox.
                    </p>
                  }
                </div>
              } @else {
                <p class="text-sm text-gray-500">
                  Please
                  <a routerLink="/login" class="text-blue-600 hover:text-blue-500">sign in</a>
                  to request a new verification email.
                </p>
              }
            </div>
          }

          <div class="mt-6 text-center">
            @if (authService.isAuthenticated()) {
              <a routerLink="/dashboard" class="text-sm text-blue-600 hover:text-blue-500">
                Go to Dashboard
              </a>
            } @else {
              <a routerLink="/login" class="text-sm text-blue-600 hover:text-blue-500">
                Back to sign in
              </a>
            }
          </div>
        </div>
      </div>
    </div>
  `,
})
export class VerifyEmailComponent implements OnInit, OnDestroy {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroy$ = new Subject<void>();
  private resendTimer: ReturnType<typeof setInterval> | null = null;

  isVerifying = signal(false);
  verificationSuccess = signal(false);
  errorMessage = signal<string | null>(null);
  canResend = signal(false);
  isResending = signal(false);
  resendSuccess = signal(false);
  resendCooldown = signal(0);

  ngOnInit(): void {
    // Get token from URL and auto-verify
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const token = params['token'];
      if (token) {
        this.verifyEmail(token);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.clearResendTimer();
  }

  private async verifyEmail(token: string): Promise<void> {
    this.isVerifying.set(true);
    this.errorMessage.set(null);

    try {
      await this.authService.verifyEmail(token);
      this.verificationSuccess.set(true);
    } catch (error) {
      console.error('Email verification error:', error);

      const message =
        error instanceof Error ? error.message : 'Email verification failed. Please try again.';

      if (message.toLowerCase().includes('expired')) {
        this.errorMessage.set(
          'This verification link has expired. Please request a new verification email.'
        );
        this.canResend.set(true);
      } else if (
        message.toLowerCase().includes('invalid') ||
        message.toLowerCase().includes('not found')
      ) {
        this.errorMessage.set(
          'This verification link is invalid. Please request a new verification email.'
        );
        this.canResend.set(true);
      } else if (message.toLowerCase().includes('already verified')) {
        this.verificationSuccess.set(true);
      } else {
        this.errorMessage.set(message);
        this.canResend.set(true);
      }
    } finally {
      this.isVerifying.set(false);
    }
  }

  async resendVerification(): Promise<void> {
    if (this.resendCooldown() > 0 || !this.authService.isAuthenticated()) {
      return;
    }

    this.isResending.set(true);
    this.resendSuccess.set(false);

    try {
      await this.authService.resendVerificationEmail();
      this.resendSuccess.set(true);
      this.startResendCooldown();
    } catch (error) {
      console.error('Resend verification error:', error);
      this.errorMessage.set('Failed to send verification email. Please try again.');
    } finally {
      this.isResending.set(false);
    }
  }

  navigateToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  private startResendCooldown(): void {
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
