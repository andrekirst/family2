import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { PasswordValidationResult } from '../../../../core/models/auth.models';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

/**
 * Reset password component for setting a new password.
 * Supports both token-based (link) and code-based (mobile) reset flows.
 */
@Component({
  selector: 'app-reset-password',
  imports: [FormsModule, RouterLink, ButtonComponent, InputComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 mb-2">Family Hub</h1>
          <p class="text-gray-600">Set your new password</p>
        </div>

        <div class="bg-white shadow-lg rounded-lg p-8">
          @if (!resetSuccess()) {
            <h2 class="text-2xl font-semibold text-gray-900 mb-6 text-center">Reset Password</h2>

            @if (errorMessage()) {
              <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
                <p class="text-sm text-red-600">{{ errorMessage() }}</p>
              </div>
            }

            <form (ngSubmit)="resetPassword()" class="space-y-6">
              <!-- Code flow: Show email and code inputs -->
              @if (isCodeFlow()) {
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

                <div>
                  <label for="code" class="block text-sm font-medium text-gray-700 mb-2">
                    Reset Code
                  </label>
                  <app-input
                    id="code"
                    type="text"
                    placeholder="Enter 6-digit code"
                    [(ngModel)]="code"
                    name="code"
                    [required]="true"
                    autocomplete="one-time-code"
                    maxlength="6"
                    class="w-full"
                  />
                  <p class="mt-1 text-xs text-gray-500">Enter the 6-digit code from your email</p>
                </div>
              }

              <div>
                <label for="newPassword" class="block text-sm font-medium text-gray-700 mb-2">
                  New Password
                </label>
                <div class="relative">
                  <app-input
                    id="newPassword"
                    [type]="showPassword() ? 'text' : 'password'"
                    placeholder="Enter your new password"
                    [(ngModel)]="newPassword"
                    (ngModelChange)="onPasswordChange($event)"
                    name="newPassword"
                    [required]="true"
                    autocomplete="new-password"
                    class="w-full"
                  />
                  <button
                    type="button"
                    (click)="togglePasswordVisibility()"
                    class="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600"
                  >
                    @if (showPassword()) {
                      <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
                        />
                      </svg>
                    } @else {
                      <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                        />
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                        />
                      </svg>
                    }
                  </button>
                </div>

                <!-- Password Strength Meter -->
                @if (newPassword && passwordValidation()) {
                  <div class="mt-2">
                    <div class="flex items-center justify-between mb-1">
                      <span class="text-xs text-gray-500">Password strength:</span>
                      <span
                        class="text-xs font-medium"
                        [class.text-red-600]="passwordValidation()!.score <= 1"
                        [class.text-yellow-600]="passwordValidation()!.score === 2"
                        [class.text-green-600]="passwordValidation()!.score >= 3"
                      >
                        {{ passwordValidation()!.strength }}
                      </span>
                    </div>
                    <div class="w-full bg-gray-200 rounded-full h-2">
                      <div
                        class="h-2 rounded-full transition-all duration-300"
                        [class.bg-red-500]="passwordValidation()!.score <= 1"
                        [class.bg-yellow-500]="passwordValidation()!.score === 2"
                        [class.bg-green-500]="passwordValidation()!.score >= 3"
                        [style.width.%]="(passwordValidation()!.score / 4) * 100"
                      ></div>
                    </div>

                    @if (passwordValidation()!.errors.length > 0) {
                      <ul class="mt-2 text-xs text-red-600 space-y-1">
                        @for (error of passwordValidation()!.errors; track error) {
                          <li class="flex items-start">
                            <svg
                              class="h-4 w-4 mr-1 flex-shrink-0"
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
                            {{ error }}
                          </li>
                        }
                      </ul>
                    }
                  </div>
                }
              </div>

              <div>
                <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-2">
                  Confirm New Password
                </label>
                <div class="relative">
                  <app-input
                    id="confirmPassword"
                    [type]="showConfirmPassword() ? 'text' : 'password'"
                    placeholder="Confirm your new password"
                    [(ngModel)]="confirmPassword"
                    name="confirmPassword"
                    [required]="true"
                    autocomplete="new-password"
                    class="w-full"
                  />
                  <button
                    type="button"
                    (click)="toggleConfirmPasswordVisibility()"
                    class="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600"
                  >
                    @if (showConfirmPassword()) {
                      <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
                        />
                      </svg>
                    } @else {
                      <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                        />
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                        />
                      </svg>
                    }
                  </button>
                </div>
                @if (confirmPassword && newPassword !== confirmPassword) {
                  <p class="mt-1 text-xs text-red-600">Passwords do not match</p>
                }
              </div>

              <app-button
                type="submit"
                variant="primary"
                size="lg"
                [loading]="isLoading()"
                [disabled]="!canSubmit()"
                class="w-full"
              >
                Reset Password
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
              <h2 class="text-2xl font-semibold text-gray-900 mb-2">Password Reset!</h2>
              <p class="text-gray-600 mb-6">
                Your password has been successfully reset. You can now sign in with your new
                password.
              </p>
              <app-button variant="primary" size="lg" (click)="navigateToLogin()" class="w-full">
                Sign In
              </app-button>
            </div>
          }

          @if (!resetSuccess()) {
            <div class="mt-6 text-center">
              <a routerLink="/login" class="text-sm text-blue-600 hover:text-blue-500">
                Back to sign in
              </a>
            </div>
          }
        </div>
      </div>
    </div>
  `,
})
export class ResetPasswordComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroy$ = new Subject<void>();
  private readonly passwordChange$ = new Subject<string>();

  // From URL params
  token = '';
  email = '';
  code = '';
  isCodeFlow = signal(false);

  newPassword = '';
  confirmPassword = '';
  showPassword = signal(false);
  showConfirmPassword = signal(false);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  resetSuccess = signal(false);
  passwordValidation = signal<PasswordValidationResult | null>(null);

  canSubmit = computed(() => {
    const validation = this.passwordValidation();
    const baseValid =
      this.newPassword.length > 0 &&
      this.confirmPassword.length > 0 &&
      this.newPassword === this.confirmPassword &&
      validation?.isValid === true &&
      !this.isLoading();

    if (this.isCodeFlow()) {
      return baseValid && this.email.trim().length > 0 && this.code.trim().length === 6;
    } else {
      return baseValid && this.token.length > 0;
    }
  });

  ngOnInit(): void {
    // Get params from URL
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      this.token = params['token'] || '';
      this.email = params['email'] || '';
      this.isCodeFlow.set(params['method'] === 'code' || !this.token);
    });

    // Setup debounced password validation
    this.passwordChange$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((password) => {
        this.validatePassword(password);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  togglePasswordVisibility(): void {
    this.showPassword.update((value) => !value);
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update((value) => !value);
  }

  onPasswordChange(password: string): void {
    this.passwordChange$.next(password);
  }

  private async validatePassword(password: string): Promise<void> {
    if (!password) {
      this.passwordValidation.set(null);
      return;
    }

    try {
      const result = await this.authService.validatePassword(password);
      this.passwordValidation.set(result);
    } catch (error) {
      console.error('Password validation error:', error);
      this.passwordValidation.set({
        isValid: false,
        score: 0,
        strength: 'Unknown',
        errors: ['Unable to validate password'],
        suggestions: [],
      });
    }
  }

  async resetPassword(): Promise<void> {
    if (!this.canSubmit()) {
      return;
    }

    try {
      this.isLoading.set(true);
      this.errorMessage.set(null);

      if (this.isCodeFlow()) {
        await this.authService.resetPasswordWithCode(
          this.email.trim(),
          this.code.trim(),
          this.newPassword,
          this.confirmPassword
        );
      } else {
        await this.authService.resetPassword(this.token, this.newPassword, this.confirmPassword);
      }

      this.resetSuccess.set(true);
    } catch (error) {
      console.error('Password reset error:', error);
      this.isLoading.set(false);

      const message =
        error instanceof Error ? error.message : 'Password reset failed. Please try again.';

      if (
        message.toLowerCase().includes('expired') ||
        message.toLowerCase().includes('invalid token')
      ) {
        this.errorMessage.set(
          'This reset link has expired or is invalid. Please request a new password reset.'
        );
      } else if (message.toLowerCase().includes('code')) {
        this.errorMessage.set('Invalid or expired code. Please check and try again.');
      } else {
        this.errorMessage.set(message);
      }
    }
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }
}
