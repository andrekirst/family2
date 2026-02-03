import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';

/**
 * OAuth callback component - handles redirect from Keycloak
 * Exchanges authorization code for tokens
 */
@Component({
  selector: 'app-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="text-center">
        @if (error) {
          <div class="text-red-600">
            <h2 class="text-2xl font-bold">Authentication Failed</h2>
            <p class="mt-2">{{ error }}</p>
            <button
              (click)="retry()"
              class="mt-4 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600"
            >
              Try Again
            </button>
          </div>
        } @else {
          <div class="text-gray-600">
            <div
              class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"
            ></div>
            <p class="mt-4">Completing sign in...</p>
          </div>
        }
      </div>
    </div>
  `,
})
export class CallbackComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  error: string | null = null;

  ngOnInit(): void {
    this.route.queryParams.subscribe(async (params) => {
      const code = params['code'];
      const state = params['state'];
      const error = params['error'];

      if (error) {
        this.error = `Authentication error: ${error}`;
        return;
      }

      if (!code || !state) {
        this.error = 'Missing authorization code or state parameter';
        return;
      }

      try {
        await this.authService.handleCallback(code, state);
        // Navigation handled by authService
      } catch (err: any) {
        this.error = err.message || 'Failed to complete authentication';
        console.error('Callback error:', err);
      }
    });
  }

  retry(): void {
    this.authService.login();
  }
}
