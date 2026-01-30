import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { GraphQLService } from '../shared/graphql.service';
import { REQUEST_PASSWORD_RESET_MUTATION } from '../graphql/mutations';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-base-200 px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body">
          <h2 class="card-title text-2xl font-bold mb-4">Forgot Password</h2>
          <p class="text-gray-600 mb-4">
            Enter your email and we'll send you a password reset link.
          </p>

          @if (success()) {
            <div class="alert alert-success mb-4">
              <span>{{ success() }}</span>
            </div>
            <div class="text-center mt-4">
              <a routerLink="/login" class="btn btn-primary">Back to Login</a>
            </div>
          } @else {
            @if (error()) {
              <div class="alert alert-error mb-4">
                <span>{{ error() }}</span>
              </div>
            }

            <form (ngSubmit)="onSubmit()" #forgotForm="ngForm">
              <div class="form-control w-full">
                <label class="label">
                  <span class="label-text">Email</span>
                </label>
                <input
                  type="email"
                  placeholder="you@example.com"
                  class="input input-bordered w-full"
                  [(ngModel)]="email"
                  name="email"
                  required
                  email
                  [disabled]="loading()"
                />
              </div>

              <div class="form-control mt-6">
                <button
                  type="submit"
                  class="btn btn-primary"
                  [disabled]="loading() || forgotForm.invalid"
                >
                  @if (loading()) {
                    <span class="loading loading-spinner"></span>
                    Sending...
                  } @else {
                    Send Reset Link
                  }
                </button>
              </div>
            </form>

            <div class="text-center mt-4">
              <a routerLink="/login" class="link link-primary text-sm">Back to Login</a>
            </div>
          }
        </div>
      </div>
    </div>
  `,
})
export class ForgotPasswordComponent {
  email = '';
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  constructor(private graphql: GraphQLService) {}

  async onSubmit() {
    if (!this.email) return;

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    try {
      const result: any = await this.graphql.mutate(REQUEST_PASSWORD_RESET_MUTATION, {
        input: {
          input: {
            email: this.email,
          },
        },
      });

      const resetResult = result.requestPasswordReset.mutationResultOfBoolean;

      if (resetResult.success) {
        this.success.set('Password reset link sent! Check your email.');
      } else {
        this.error.set(resetResult.error?.message || 'Failed to send reset link');
      }
    } catch (err: any) {
      console.error('Forgot password error:', err);
      this.error.set(err.message || 'An error occurred');
    } finally {
      this.loading.set(false);
    }
  }
}
