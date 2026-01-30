import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { GraphQLService } from '../shared/graphql.service';
import { REGISTER_MUTATION } from '../graphql/mutations';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-base-200 px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body">
          <h2 class="card-title text-2xl font-bold mb-4">Create Account</h2>

          @if (success()) {
            <div class="alert alert-success mb-4">
              <span>{{ success() }}</span>
            </div>
          }

          @if (error()) {
            <div class="alert alert-error mb-4">
              <span>{{ error() }}</span>
            </div>
          }

          @if (!success()) {
            <form (ngSubmit)="onSubmit()" #registerForm="ngForm">
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

              <div class="form-control w-full mt-4">
                <label class="label">
                  <span class="label-text">Password</span>
                </label>
                <input
                  type="password"
                  placeholder="At least 8 characters"
                  class="input input-bordered w-full"
                  [(ngModel)]="password"
                  name="password"
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
                  placeholder="Re-enter password"
                  class="input input-bordered w-full"
                  [(ngModel)]="confirmPassword"
                  name="confirmPassword"
                  required
                  [disabled]="loading()"
                />
                @if (password && confirmPassword && password !== confirmPassword) {
                  <label class="label">
                    <span class="label-text-alt text-error">Passwords do not match</span>
                  </label>
                }
              </div>

              <div class="form-control mt-6">
                <button
                  type="submit"
                  class="btn btn-primary"
                  [disabled]="loading() || registerForm.invalid || password !== confirmPassword"
                >
                  @if (loading()) {
                    <span class="loading loading-spinner"></span>
                    Creating account...
                  } @else {
                    Register
                  }
                </button>
              </div>
            </form>

            <div class="divider">OR</div>

            <div class="text-center">
              <span class="text-sm">Already have an account? </span>
              <a routerLink="/login" class="link link-primary text-sm">Login</a>
            </div>
          } @else {
            <div class="text-center mt-4">
              <a routerLink="/login" class="btn btn-primary">Go to Login</a>
            </div>
          }
        </div>
      </div>
    </div>
  `,
})
export class RegisterComponent {
  email = '';
  password = '';
  confirmPassword = '';
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  constructor(
    private graphql: GraphQLService,
    private router: Router,
  ) {}

  async onSubmit() {
    if (!this.email || !this.password || this.password !== this.confirmPassword) return;

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    try {
      const result: any = await this.graphql.mutate(REGISTER_MUTATION, {
        input: {
          input: {
            email: this.email,
            password: this.password,
            confirmPassword: this.confirmPassword,
          },
        },
      });

      const registerResult = result.register.mutationResultOfRegisterResult;

      if (registerResult.success && registerResult.data) {
        this.success.set(
          registerResult.data.message ||
            'Registration successful! Please check your email to verify your account.',
        );
      } else {
        this.error.set(registerResult.error?.message || 'Registration failed');
      }
    } catch (err: any) {
      console.error('Register error:', err);
      this.error.set(err.message || 'An error occurred during registration');
    } finally {
      this.loading.set(false);
    }
  }
}
