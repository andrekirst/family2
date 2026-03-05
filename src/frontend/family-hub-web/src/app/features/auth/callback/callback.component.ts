import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { UserService } from '../../../core/user/user.service';
import { CommonModule } from '@angular/common';

/**
 * OAuth callback component - handles redirect from Keycloak.
 * Exchanges authorization code for tokens, syncs user with backend,
 * and navigates to the dashboard.
 * Other consumers (dashboard, guards) use whenReady() to share
 * the same in-flight request on F5 refresh.
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
            <p class="mt-4" i18n="@@callback.signingIn">Signing you in...</p>
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
  private destroyRef = inject(DestroyRef);

  error: string | null = null;

  ngOnInit(): void {
    this.route.queryParams
      .pipe(take(1), takeUntilDestroyed(this.destroyRef))
      .subscribe(async (params) => {
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
          // Exchange authorization code for tokens (OAuth PKCE flow)
          await this.authService.handleCallback(code, state);

          // Sync user with backend (also populates _readyPromise for whenReady())
          await this.userService.registerUser();

          // Navigate to intended destination
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
