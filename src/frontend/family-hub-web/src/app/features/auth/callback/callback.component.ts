import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { UserService } from '../../../core/user/user.service';
import { HealthService } from '../../../shared/services/health.service';
import { CommonModule } from '@angular/common';
import { firstValueFrom } from 'rxjs';

/**
 * OAuth callback component - handles redirect from Keycloak
 * Exchanges authorization code for tokens, verifies backend health, then syncs with backend
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
            <div class="mt-4 flex justify-center gap-3">
              <button
                (click)="retry()"
                class="px-4 py-2 bg-primary text-white rounded hover:bg-blue-600"
              >
                <span i18n="@@callback.tryAgain">Try Again</span>
              </button>
              <a
                routerLink="/status"
                class="px-4 py-2 bg-gray-200 text-gray-700 rounded hover:bg-gray-300 inline-flex items-center"
              >
                <span>View System Status</span>
              </a>
            </div>
          </div>
        } @else {
          <div class="text-gray-600">
            <div
              class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"
            ></div>
            <div class="mt-4 space-y-2">
              <p [class.text-green-600]="step() >= 1">
                {{ step() >= 1 ? '&#10003;' : '&#9675;' }}
                <span i18n="@@callback.exchangingCode">Exchanging authorization code...</span>
              </p>
              <p [class.text-green-600]="step() >= 2">
                {{ step() >= 2 ? '&#10003;' : '&#9675;' }}
                <span i18n="@@callback.verifyingBackend">Verifying backend services...</span>
              </p>
              <p [class.text-green-600]="step() >= 3">
                {{ step() >= 3 ? '&#10003;' : '&#9675;' }}
                <span i18n="@@callback.syncingBackend">Syncing with backend...</span>
              </p>
              <p [class.text-green-600]="step() >= 4">
                {{ step() >= 4 ? '&#10003;' : '&#9675;' }}
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
  private healthService = inject(HealthService);

  error: string | null = null;
  step = signal(1); // Track progress through authentication flow

  ngOnInit(): void {
    this.route.queryParams.subscribe(async (params) => {
      // If already authenticated (e.g., after HMR re-render), skip and redirect
      if (this.authService.isAuthenticated()) {
        const redirectUrl = this.authService.consumePostLoginRedirect();
        await this.router.navigateByUrl(redirectUrl);
        return;
      }

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

        // Step 2: Verify backend health before calling RegisterUser
        const health = await firstValueFrom(this.healthService.checkHealth());
        if (health.status !== 'Healthy') {
          await this.router.navigateByUrl('/status?from=callback');
          return;
        }
        this.step.set(3);

        // Step 3: Sync user with backend database
        await this.userService.registerUser();
        this.step.set(4);

        // Step 4: Navigate to intended destination (after registration completes)
        const redirectUrl = this.authService.consumePostLoginRedirect();
        const separator = redirectUrl.includes('?') ? '&' : '?';
        await this.router.navigateByUrl(`${redirectUrl}${separator}login=success`);
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
