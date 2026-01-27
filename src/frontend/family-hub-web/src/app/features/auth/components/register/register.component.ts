import { Component, inject, signal, computed, OnDestroy } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { PasswordValidationResult } from '../../../../core/models/auth.models';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

/**
 * Register component for local email/password registration.
 * Provides registration form with real-time password strength validation.
 */
@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink, ButtonComponent, InputComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4 py-8">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 mb-2">Family Hub</h1>
          <p class="text-gray-600">Create your account to get started</p>
        </div>

        <div class="bg-white shadow-lg rounded-lg p-8">
          <h2 class="text-2xl font-semibold text-gray-900 mb-6 text-center">Create Account</h2>

          @if (errorMessage()) {
            <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
              <p class="text-sm text-red-600">{{ errorMessage() }}</p>
            </div>
          }

          <form (ngSubmit)="register()" class="space-y-6">
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
              <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
                Password
              </label>
              <div class="relative">
                <app-input
                  id="password"
                  [type]="showPassword() ? 'text' : 'password'"
                  placeholder="Create a strong password"
                  [(ngModel)]="password"
                  (ngModelChange)="onPasswordChange($event)"
                  name="password"
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
              @if (password && passwordValidation()) {
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

                  <!-- Validation Errors -->
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

                  <!-- Suggestions -->
                  @if (
                    passwordValidation()!.suggestions.length > 0 &&
                    passwordValidation()!.errors.length === 0
                  ) {
                    <ul class="mt-2 text-xs text-gray-500 space-y-1">
                      @for (suggestion of passwordValidation()!.suggestions; track suggestion) {
                        <li class="flex items-start">
                          <svg
                            class="h-4 w-4 mr-1 flex-shrink-0 text-blue-500"
                            fill="none"
                            viewBox="0 0 24 24"
                            stroke="currentColor"
                          >
                            <path
                              stroke-linecap="round"
                              stroke-linejoin="round"
                              stroke-width="2"
                              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                            />
                          </svg>
                          {{ suggestion }}
                        </li>
                      }
                    </ul>
                  }
                </div>
              }
            </div>

            <div>
              <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-2">
                Confirm Password
              </label>
              <div class="relative">
                <app-input
                  id="confirmPassword"
                  [type]="showConfirmPassword() ? 'text' : 'password'"
                  placeholder="Confirm your password"
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
              @if (confirmPassword && password !== confirmPassword) {
                <p class="mt-1 text-xs text-red-600">Passwords do not match</p>
              }
            </div>

            <div class="flex items-start">
              <input
                id="terms"
                type="checkbox"
                [(ngModel)]="acceptTerms"
                name="terms"
                class="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 mt-0.5"
              />
              <label for="terms" class="ml-2 text-sm text-gray-600">
                I agree to the
                <a href="/terms" class="text-blue-600 hover:text-blue-500">Terms of Service</a>
                and
                <a href="/privacy" class="text-blue-600 hover:text-blue-500">Privacy Policy</a>
              </label>
            </div>

            <app-button
              type="submit"
              variant="primary"
              size="lg"
              [loading]="isLoading()"
              [disabled]="!canSubmit()"
              class="w-full"
            >
              Create Account
            </app-button>
          </form>

          <div class="mt-6">
            <div class="relative">
              <div class="absolute inset-0 flex items-center">
                <div class="w-full border-t border-gray-300"></div>
              </div>
              <div class="relative flex justify-center text-sm">
                <span class="px-2 bg-white text-gray-500">Already have an account?</span>
              </div>
            </div>

            <div class="mt-6">
              <a
                routerLink="/login"
                class="w-full flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                Sign in instead
              </a>
            </div>
          </div>
        </div>

        <p class="text-xs text-gray-500 text-center">
          By creating an account, you agree to our Terms of Service and Privacy Policy.
        </p>
      </div>
    </div>
  `,
})
export class RegisterComponent implements OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();
  private readonly passwordChange$ = new Subject<string>();

  email = '';
  password = '';
  confirmPassword = '';
  acceptTerms = false;
  showPassword = signal(false);
  showConfirmPassword = signal(false);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  passwordValidation = signal<PasswordValidationResult | null>(null);
  isValidatingPassword = signal(false);

  // Computed signal to determine if form can be submitted
  canSubmit = computed(() => {
    const validation = this.passwordValidation();
    return (
      this.email.trim().length > 0 &&
      this.password.length > 0 &&
      this.confirmPassword.length > 0 &&
      this.password === this.confirmPassword &&
      this.acceptTerms &&
      validation?.isValid === true &&
      !this.isLoading()
    );
  });

  constructor() {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }

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
      this.isValidatingPassword.set(true);
      const result = await this.authService.validatePassword(password);
      this.passwordValidation.set(result);
    } catch (error) {
      console.error('Password validation error:', error);
      // Set a default validation result on error
      this.passwordValidation.set({
        isValid: false,
        score: 0,
        strength: 'Unknown',
        errors: ['Unable to validate password'],
        suggestions: [],
      });
    } finally {
      this.isValidatingPassword.set(false);
    }
  }

  async register(): Promise<void> {
    if (!this.canSubmit()) {
      return;
    }

    try {
      this.isLoading.set(true);
      this.errorMessage.set(null);

      await this.authService.register(this.email.trim(), this.password, this.confirmPassword);

      // Navigate to dashboard on success
      // Note: If email verification is required, the dashboard or a guard will handle that
      await this.router.navigate(['/dashboard']);
    } catch (error) {
      console.error('Registration error:', error);
      this.isLoading.set(false);

      // Handle specific error messages
      const message =
        error instanceof Error ? error.message : 'Registration failed. Please try again.';

      if (message.toLowerCase().includes('email') && message.toLowerCase().includes('exists')) {
        this.errorMessage.set('An account with this email already exists. Please sign in instead.');
      } else if (message.toLowerCase().includes('password')) {
        this.errorMessage.set(message);
      } else {
        this.errorMessage.set(message);
      }
    }
  }
}
