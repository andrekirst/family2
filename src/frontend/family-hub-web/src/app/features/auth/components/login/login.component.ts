import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';

@Component({
  selector: 'app-login',
  imports: [FormsModule, ButtonComponent, InputComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 mb-2">Family Hub</h1>
          <p class="text-gray-600">Organize your family life with ease</p>
        </div>

        <div class="bg-white shadow-lg rounded-lg p-8">
          <h2 class="text-2xl font-semibold text-gray-900 mb-6 text-center">Welcome Back</h2>

          <form (ngSubmit)="login()" class="space-y-6">
            <div>
              <label for="identifier" class="block text-sm font-medium text-gray-700 mb-2">
                Email Address
              </label>
              <app-input
                id="identifier"
                type="email"
                placeholder="Enter your email address"
                [(ngModel)]="identifier"
                name="identifier"
                [required]="true"
                autocomplete="username"
                class="w-full"
              />
              @if (errorMessage()) {
                <p class="mt-2 text-sm text-red-600">{{ errorMessage() }}</p>
              }
            </div>

            <app-button
              type="submit"
              variant="primary"
              size="lg"
              [loading]="isLoading()"
              [disabled]="!identifier.trim()"
              class="w-full"
            >
              Sign in
            </app-button>
          </form>

          <p class="mt-4 text-sm text-gray-500 text-center">
            Secure authentication powered by Zitadel
          </p>
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

  identifier = '';
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  async login(): Promise<void> {
    if (!this.identifier.trim()) {
      this.errorMessage.set('Please enter your email or username');
      return;
    }

    try {
      this.isLoading.set(true);
      this.errorMessage.set(null);
      await this.authService.login(this.identifier.trim());
      // Will redirect to Zitadel, so loading state stays true
    } catch (error) {
      console.error('Login error:', error);
      this.isLoading.set(false);
      this.errorMessage.set(
        error instanceof Error ? error.message : 'Login failed. Please try again.'
      );
    }
  }
}
