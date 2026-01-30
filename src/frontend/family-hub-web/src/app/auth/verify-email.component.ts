import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { GraphQLService } from '../shared/graphql.service';
import { VERIFY_EMAIL_MUTATION } from '../graphql/mutations';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-base-200 px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body text-center">
          @if (loading()) {
            <span class="loading loading-spinner loading-lg"></span>
            <p class="mt-4">Verifying your email...</p>
          } @else if (success()) {
            <div class="text-success">
              <svg class="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M5 13l4 4L19 7"
                ></path>
              </svg>
            </div>
            <h2 class="card-title text-2xl font-bold mt-4">Email Verified!</h2>
            <p class="text-gray-600 mt-2">Your email has been successfully verified.</p>
            <div class="mt-6">
              <a routerLink="/login" class="btn btn-primary">Go to Login</a>
            </div>
          } @else if (error()) {
            <div class="text-error">
              <svg class="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                ></path>
              </svg>
            </div>
            <h2 class="card-title text-2xl font-bold mt-4">Verification Failed</h2>
            <p class="text-gray-600 mt-2">{{ error() }}</p>
            <div class="mt-6">
              <a routerLink="/login" class="btn btn-primary">Back to Login</a>
            </div>
          }
        </div>
      </div>
    </div>
  `,
})
export class VerifyEmailComponent implements OnInit {
  loading = signal(true);
  success = signal(false);
  error = signal<string | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private graphql: GraphQLService,
  ) {}

  ngOnInit() {
    // Get token from URL query params
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.error.set('No verification token provided');
      this.loading.set(false);
      return;
    }

    this.verifyEmail(token);
  }

  async verifyEmail(token: string) {
    try {
      const result: any = await this.graphql.mutate(VERIFY_EMAIL_MUTATION, {
        input: {
          input: {
            token,
          },
        },
      });

      const verifyResult = result.verifyEmail.mutationResultOfVerifyEmailResult;

      if (verifyResult.success) {
        this.success.set(true);
      } else {
        this.error.set(verifyResult.error?.message || 'Verification failed');
      }
    } catch (err: any) {
      console.error('Verify email error:', err);
      this.error.set(err.message || 'An error occurred during verification');
    } finally {
      this.loading.set(false);
    }
  }
}
