import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { UserService } from '../../../core/user/user.service';
import { CommonModule } from '@angular/common';

/**
 * OAuth callback component - handles redirect from Keycloak
 * Exchanges authorization code for tokens and syncs with backend
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
            <h2 class="text-2xl font-bold" i18n="@@callback.authFailed">Authentication Failed</h2>
            <p class="mt-2">{{ error }}</p>
            <button
              (click)="retry()"
              class="mt-4 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600"
            >
              <span i18n="@@callback.tryAgain">Try Again</span>
            </button>
          </div>
        } @else {
          <div class="text-gray-600">
            <div
              class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"
            ></div>
            <div class="mt-4 space-y-2">
              <p [class.text-green-600]="step() >= 1">
                {{ step() >= 1 ? '✓' : '○' }}
                <span i18n="@@callback.exchangingCode">Exchanging authorization code...</span>
              </p>
              <p [class.text-green-600]="step() >= 2">
                {{ step() >= 2 ? '✓' : '○' }}
                <span i18n="@@callback.syncingBackend">Syncing with backend...</span>
              </p>
              <p [class.text-green-600]="step() >= 3">
                {{ step() >= 3 ? '✓' : '○' }}
                <span i18n="@@callback.loadingDashboard">Loading dashboard...</span>
              </p>
            </div>
          </div>
        }
      </div>
    </div>
  `,
})
export class CallbackComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);
  private userService = inject(UserService);

  error: string | null = null;
  step = signal(1); // Track progress through authentication flow

  ngOnInit(): void {
    this.route.queryParams.subscribe(async (params) => {
      const code = params['code'];
      const state = params['state'];
      const error = params['error'];

      if (error) {
        this.error = $localize`:@@callback.authError:Authentication error` + `: ${error}`;
        return;
      }

      if (!code || !state) {
        this.error = $localize`:@@callback.missingParams:Missing authorization code or state parameter`;
        return;
      }

      try {
        // Step 1: Exchange authorization code for tokens (OAuth PKCE flow)
        await this.authService.handleCallback(code, state);
        this.step.set(2);

        // Step 2: Sync user with backend database
        await this.userService.registerUser();
        this.step.set(3);

        // Step 3: Navigate to intended destination (after registration completes)
        const redirectUrl = this.authService.consumePostLoginRedirect();
        await this.router.navigateByUrl(`${redirectUrl}?login=success`);
      } catch (err: any) {
        this.error =
          err.message || $localize`:@@callback.failedAuth:Failed to complete authentication`;
        console.error('Callback error:', err);
      }
    });
  }

  retry(): void {
    this.authService.login();
  }
}
