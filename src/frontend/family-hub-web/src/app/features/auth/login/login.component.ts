import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/auth/auth.service';

/**
 * Login component - initiates OAuth 2.0 flow with Keycloak
 */
@Component({
  selector: 'app-login',
  standalone: true,
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="max-w-md w-full space-y-8 p-8">
        <div class="text-center">
          <h2 class="text-3xl font-bold text-gray-900">Family Hub</h2>
          <p class="mt-2 text-sm text-gray-600">Organize your family life in one place</p>
        </div>

        <div class="mt-8">
          <button
            (click)="login()"
            class="w-full flex justify-center py-3 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
          >
            Sign in with Keycloak
          </button>
        </div>

        <p class="mt-4 text-xs text-center text-gray-500">
          By signing in, you agree to our Terms of Service and Privacy Policy
        </p>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private authService = inject(AuthService);

  login(): void {
    this.authService.login();
  }
}
