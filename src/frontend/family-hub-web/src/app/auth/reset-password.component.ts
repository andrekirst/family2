import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { GraphQLService } from '../shared/graphql.service';
import { RESET_PASSWORD_MUTATION } from '../graphql/mutations';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-base-200 px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body">
          <h2 class="card-title text-2xl font-bold mb-4">Reset Password</h2>

          @if (success()) {
            <div class="alert alert-success mb-4">
              <span>Password reset successfully!</span>
            </div>
            <div class="text-center mt-4">
              <a routerLink="/login" class="btn btn-primary">Go to Login</a>
            </div>
          } @else {
            @if (error()) {
              <div class="alert alert-error mb-4">
                <span>{{ error() }}</span>
              </div>
            }

            @if (!token()) {
              <div class="alert alert-warning">
                <span>Invalid or missing reset token</span>
              </div>
              <div class="text-center mt-4">
                <a routerLink="/forgot-password" class="btn btn-primary">Request New Link</a>
              </div>
            } @else {
              <form (ngSubmit)="onSubmit()" #resetForm="ngForm">
                <div class="form-control w-full">
                  <label class="label">
                    <span class="label-text">New Password</span>
                  </label>
                  <input
                    type="password"
                    placeholder="At least 8 characters"
                    class="input input-bordered w-full"
                    [(ngModel)]="newPassword"
                    name="newPassword"
                    required
                    minlength="8"
                    [disabled]="loading()"
                  />
                </div>

                <div class="form-control w-full mt-4">
                  <label class="label">
                    <span class="label-text">Confirm Password</span>
                  </label>
                  <input
                    type="password"
                    placeholder="Re-enter new password"
                    class="input input-bordered w-full"
                    [(ngModel)]="confirmPassword"
                    name="confirmPassword"
                    required
                    [disabled]="loading()"
                  />
                  @if (newPassword && confirmPassword && newPassword !== confirmPassword) {
                    <label class="label">
                      <span class="label-text-alt text-error">Passwords do not match</span>
                    </label>
                  }
                </div>

                <div class="form-control mt-6">
                  <button
                    type="submit"
                    class="btn btn-primary"
                    [disabled]="loading() || resetForm.invalid || newPassword !== confirmPassword"
                  >
                    @if (loading()) {
                      <span class="loading loading-spinner"></span>
                      Resetting...
                    } @else {
                      Reset Password
                    }
                  </button>
                </div>
              </form>

              <div class="text-center mt-4">
                <a routerLink="/login" class="link link-primary text-sm">Back to Login</a>
              </div>
            }
          }
        </div>
      </div>
    </div>
  `,
})
export class ResetPasswordComponent implements OnInit {
  token = signal<string | null>(null);
  newPassword = '';
  confirmPassword = '';
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private graphql: GraphQLService,
  ) {}

  ngOnInit() {
    // Get token from URL query params
    const tokenParam = this.route.snapshot.queryParamMap.get('token');
    this.token.set(tokenParam);
  }

  async onSubmit() {
    if (!this.token() || !this.newPassword || this.newPassword !== this.confirmPassword) return;

    this.loading.set(true);
    this.error.set(null);

    try {
      const result: any = await this.graphql.mutate(RESET_PASSWORD_MUTATION, {
        input: {
          input: {
            token: this.token(),
            newPassword: this.newPassword,
            confirmPassword: this.confirmPassword,
          },
        },
      });

      const resetResult = result.resetPassword.mutationResultOfBoolean;

      if (resetResult.success) {
        this.success.set(true);
      } else {
        this.error.set(resetResult.error?.message || 'Password reset failed');
      }
    } catch (err: any) {
      console.error('Reset password error:', err);
      this.error.set(err.message || 'An error occurred');
    } finally {
      this.loading.set(false);
    }
  }
}
