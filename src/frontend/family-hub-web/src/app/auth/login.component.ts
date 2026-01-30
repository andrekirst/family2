import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { GraphQLService } from '../shared/graphql.service';
import { AuthService } from './auth.service';
import { LOGIN_MUTATION } from '../graphql/mutations';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-base-200 px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body">
          <h2 class="card-title text-2xl font-bold mb-4">Login to Family Hub</h2>

          @if (error()) {
            <div class="alert alert-error mb-4">
              <span>{{ error() }}</span>
            </div>
          }

          <form (ngSubmit)="onSubmit()" #loginForm="ngForm">
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
                [disabled]="loading()"
              />
            </div>

            <div class="form-control w-full mt-4">
              <label class="label">
                <span class="label-text">Password</span>
              </label>
              <input
                type="password"
                placeholder="Enter your password"
                class="input input-bordered w-full"
                [(ngModel)]="password"
                name="password"
                required
                [disabled]="loading()"
              />
              <label class="label">
                <a routerLink="/forgot-password" class="label-text-alt link link-hover"
                  >Forgot password?</a
                >
              </label>
            </div>

            <div class="form-control mt-6">
              <button
                type="submit"
                class="btn btn-primary"
                [disabled]="loading() || loginForm.invalid"
              >
                @if (loading()) {
                  <span class="loading loading-spinner"></span>
                  Logging in...
                } @else {
                  Login
                }
              </button>
            </div>
          </form>

          <div class="divider">OR</div>

          <div class="text-center">
            <span class="text-sm">Don't have an account? </span>
            <a routerLink="/register" class="link link-primary text-sm">Register</a>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class LoginComponent {
  email = '';
  password = '';
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(
    private graphql: GraphQLService,
    private auth: AuthService,
    private router: Router,
  ) {}

  async onSubmit() {
    if (!this.email || !this.password) return;

    this.loading.set(true);
    this.error.set(null);

    try {
      const result: any = await this.graphql.mutate(LOGIN_MUTATION, {
        input: {
          input: {
            email: this.email,
            password: this.password,
          },
        },
      });

      const loginResult = result.login.mutationResultOfLoginResult;

      if (loginResult.success && loginResult.data) {
        // Save tokens
        this.auth.saveTokens(
          loginResult.data.accessToken,
          loginResult.data.refreshToken,
          loginResult.data.accessTokenExpiresAt,
        );

        // Redirect to dashboard
        this.router.navigate(['/dashboard']);
      } else {
        this.error.set(loginResult.error?.message || 'Login failed');
      }
    } catch (err: any) {
      console.error('Login error:', err);
      this.error.set(err.message || 'An error occurred during login');
    } finally {
      this.loading.set(false);
    }
  }
}
