import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';

/**
 * Login component for local email/password authentication.
 * Provides email/password form with show/hide toggle and error handling.
 */
@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink, ButtonComponent, InputComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 mb-2">Family Hub</h1>
          <p class="text-gray-600">Organize your family life with ease</p>
        </div>

        <div class="bg-white shadow-lg rounded-lg p-8">
          <h2 class="text-2xl font-semibold text-gray-900 mb-6 text-center">Welcome Back</h2>

          @if (errorMessage()) {
            <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
              <p class="text-sm text-red-600">{{ errorMessage() }}</p>
            </div>
          }

          <form (ngSubmit)="login()" class="space-y-6">
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
                autocomplete="username"
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
                  placeholder="Enter your password"
                  [(ngModel)]="password"
                  name="password"
                  [required]="true"
                  autocomplete="current-password"
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
            </div>

            <div class="flex items-center justify-end">
              <a routerLink="/forgot-password" class="text-sm text-blue-600 hover:text-blue-500">
                Forgot your password?
              </a>
            </div>

            <app-button
              type="submit"
              variant="primary"
              size="lg"
              [loading]="isLoading()"
              [disabled]="!email.trim() || !password"
              class="w-full"
            >
              Sign in
            </app-button>
          </form>

          <div class="mt-6">
            <div class="relative">
              <div class="absolute inset-0 flex items-center">
                <div class="w-full border-t border-gray-300"></div>
              </div>
              <div class="relative flex justify-center text-sm">
                <span class="px-2 bg-white text-gray-500">New to Family Hub?</span>
              </div>
            </div>

            <div class="mt-6">
              <a
                routerLink="/register"
                class="w-full flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                Create an account
              </a>
            </div>
          </div>
        </div>

        <p class="text-xs text-gray-500 text-center">
          By signing in, you agree to our Terms of Service and Privacy Policy.
        </p>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  showPassword = signal(false);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword.update((value) => !value);
  }

  async login(): Promise<void> {
    if (!this.email.trim()) {
      this.errorMessage.set('Please enter your email address');
      return;
    }

    if (!this.password) {
      this.errorMessage.set('Please enter your password');
      return;
    }

    try {
      this.isLoading.set(true);
      this.errorMessage.set(null);

      await this.authService.login(this.email.trim(), this.password);

      // Navigate to dashboard on success
      await this.router.navigate(['/dashboard']);
    } catch (error) {
      console.error('Login error:', error);
      this.isLoading.set(false);

      // Handle specific error messages
      const message = error instanceof Error ? error.message : 'Login failed. Please try again.';

      // Check for lockout
      if (message.toLowerCase().includes('locked')) {
        this.errorMessage.set('Your account is temporarily locked. Please try again later.');
      } else if (
        message.toLowerCase().includes('invalid') ||
        message.toLowerCase().includes('credentials')
      ) {
        this.errorMessage.set('Invalid email or password. Please try again.');
      } else {
        this.errorMessage.set(message);
      }
    }
  }
}
