import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';

@Component({
    selector: 'app-login',
    imports: [ButtonComponent],
    template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 mb-2">
            Family Hub
          </h1>
          <p class="text-gray-600">
            Organize your family life with ease
          </p>
        </div>

        <div class="bg-white shadow-lg rounded-lg p-8">
          <h2 class="text-2xl font-semibold text-gray-900 mb-6 text-center">
            Welcome Back
          </h2>

          <app-button
            variant="primary"
            size="lg"
            [loading]="isLoading"
            (clicked)="login()"
            class="w-full"
          >
            Sign in with Zitadel
          </app-button>

          <p class="mt-4 text-sm text-gray-500 text-center">
            Secure authentication powered by Zitadel
          </p>
        </div>

        <p class="text-xs text-gray-500 text-center">
          By signing in, you agree to our Terms of Service and Privacy Policy.
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {
  isLoading = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  async login(): Promise<void> {
    try {
      this.isLoading = true;
      await this.authService.login();
      // Will redirect to Zitadel, so loading state stays true
    } catch (error) {
      console.error('Login error:', error);
      this.isLoading = false;
      alert('Login failed. Please try again.');
    }
  }
}
