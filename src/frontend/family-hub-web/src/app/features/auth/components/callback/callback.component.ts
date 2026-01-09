import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { FamilyService } from '../../../family/services/family.service';
import { SpinnerComponent } from '../../../../shared/components/atoms/spinner/spinner.component';

@Component({
  selector: 'app-callback',
  imports: [SpinnerComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="text-center">
        @if (!error) {
          <app-spinner size="lg"></app-spinner>
        }

        @if (!error) {
          <div>
            <h2 class="mt-4 text-xl font-semibold text-gray-900">Completing sign in...</h2>
            <p class="mt-2 text-gray-600">Please wait while we verify your credentials</p>
          </div>
        }

        @if (error) {
          <div class="max-w-md mx-auto">
            <div class="bg-red-50 border border-red-200 rounded-lg p-6">
              <h2 class="text-xl font-semibold text-red-900 mb-2">Authentication Failed</h2>
              <p class="text-red-700 mb-4">{{ error }}</p>
              <button
                (click)="retry()"
                class="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition-colors"
              >
                Try Again
              </button>
            </div>
          </div>
        }
      </div>
    </div>
  `,
})
export class CallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly familyService = inject(FamilyService);

  error: string | null = null;

  async ngOnInit(): Promise<void> {
    try {
      // Extract authorization code and state from URL query params
      const code = this.route.snapshot.queryParamMap.get('code');
      const state = this.route.snapshot.queryParamMap.get('state');

      if (!code) {
        throw new Error('Missing authorization code');
      }

      if (!state) {
        throw new Error('Missing state parameter');
      }

      // Complete OAuth flow
      await this.authService.completeLogin(code, state);

      // Load family data after successful login
      await this.familyService.loadCurrentFamily();

      // Determine redirect URL based on family status
      let redirectUrl: string;
      if (this.familyService.hasFamily()) {
        // User has family - go to dashboard or return URL
        redirectUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/dashboard';
      } else {
        // User has no family - go to family creation wizard
        redirectUrl = '/family/create';
      }

      // Redirect
      this.router.navigate([redirectUrl]);
    } catch (error: unknown) {
      console.error('OAuth callback error:', error);
      this.error = error instanceof Error ? error.message : 'An unexpected error occurred';
    }
  }

  retry(): void {
    this.router.navigate(['/login']);
  }
}
