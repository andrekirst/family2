import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { SpinnerComponent } from '../../../../shared/components/atoms/spinner/spinner.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-callback',
  standalone: true,
  imports: [CommonModule, SpinnerComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="text-center">
        <app-spinner size="lg" *ngIf="!error"></app-spinner>

        <div *ngIf="!error">
          <h2 class="mt-4 text-xl font-semibold text-gray-900">
            Completing sign in...
          </h2>
          <p class="mt-2 text-gray-600">
            Please wait while we verify your credentials
          </p>
        </div>

        <div *ngIf="error" class="max-w-md mx-auto">
          <div class="bg-red-50 border border-red-200 rounded-lg p-6">
            <h2 class="text-xl font-semibold text-red-900 mb-2">
              Authentication Failed
            </h2>
            <p class="text-red-700 mb-4">{{ error }}</p>
            <button
              (click)="retry()"
              class="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition-colors"
            >
              Try Again
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class CallbackComponent implements OnInit {
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

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

      // Get return URL from query params (if set before login)
      const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/dashboard';

      // Redirect to dashboard or return URL
      this.router.navigate([returnUrl]);

    } catch (error: any) {
      console.error('OAuth callback error:', error);
      this.error = error.message || 'An unexpected error occurred';
    }
  }

  retry(): void {
    this.router.navigate(['/login']);
  }
}
