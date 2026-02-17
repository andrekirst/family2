import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { UserService } from '../../../../core/user/user.service';
import { InvitationService } from '../../services/invitation.service';
import { InvitationDto } from '../../models/invitation.models';

@Component({
  selector: 'app-invitation-accept',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="max-w-md w-full mx-4">
        @if (isLoading()) {
          <div class="bg-white shadow rounded-lg p-8 text-center">
            <div
              class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"
            ></div>
            <p class="mt-4 text-gray-600" i18n="@@family.accept.loading">
              Loading invitation details...
            </p>
          </div>
        } @else if (error()) {
          <div class="bg-white shadow rounded-lg p-8 text-center">
            <div class="text-red-500 mb-4">
              <svg class="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"
                />
              </svg>
            </div>
            <h2 class="text-xl font-semibold text-gray-900">{{ error() }}</h2>
            <a
              routerLink="/dashboard"
              class="mt-4 inline-block px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
              i18n="@@family.accept.goToDashboard"
            >
              Go to Dashboard
            </a>
          </div>
        } @else if (invitation()) {
          <div class="bg-white shadow rounded-lg overflow-hidden">
            <div class="bg-blue-600 px-6 py-8 text-center text-white">
              <h1 class="text-2xl font-bold" i18n="@@app.name">Family Hub</h1>
              <p class="mt-2 text-blue-100" i18n="@@family.accept.youveBeenInvited">
                You've been invited!
              </p>
            </div>

            <div class="p-6">
              <h2 class="text-xl font-semibold text-gray-900" i18n="@@family.accept.joinFamily">
                Join {{ invitation()!.familyName }}
              </h2>
              <p class="mt-2 text-gray-600" i18n="@@family.accept.invitedByMessage">
                <strong>{{ invitation()!.invitedByName }}</strong> has invited you to join their
                family.
              </p>

              <div class="mt-4 p-3 bg-gray-50 rounded-lg">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500" i18n="@@family.accept.role">Role:</span>
                  <span class="font-medium text-gray-900">{{ invitation()!.role }}</span>
                </div>
                <div class="flex justify-between text-sm mt-2">
                  <span class="text-gray-500" i18n="@@family.accept.expires">Expires:</span>
                  <span class="font-medium text-gray-900">{{
                    invitation()!.expiresAt | date: 'mediumDate'
                  }}</span>
                </div>
              </div>

              @if (invitation()!.status !== 'Pending') {
                <div
                  class="mt-4 p-3 bg-yellow-50 border border-yellow-200 rounded-lg text-sm text-yellow-800"
                  i18n="@@family.accept.inactive"
                >
                  This invitation is no longer active (Status: {{ invitation()!.status }}).
                </div>
              } @else if (!isAuthenticated()) {
                <!-- Not logged in: show login button -->
                <div class="mt-6">
                  <p class="text-sm text-gray-500 mb-3" i18n="@@family.accept.loginRequired">
                    Please log in to accept this invitation.
                  </p>
                  <button
                    (click)="loginToAccept()"
                    class="w-full px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
                    i18n="@@family.accept.loginToAccept"
                  >
                    Login to Accept
                  </button>
                </div>
              } @else {
                <!-- Logged in: show accept/decline -->
                @if (actionResult()) {
                  <div
                    class="mt-4 p-3 rounded-lg text-sm"
                    [class.bg-green-50]="actionResult() === 'accepted'"
                    [class.text-green-800]="actionResult() === 'accepted'"
                    [class.bg-gray-50]="actionResult() === 'declined'"
                    [class.text-gray-800]="actionResult() === 'declined'"
                  >
                    {{ actionResult() === 'accepted' ? acceptedLabel : declinedLabel }}
                  </div>
                } @else {
                  <div class="mt-6 flex gap-3">
                    <button
                      (click)="decline()"
                      [disabled]="isProcessing()"
                      class="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
                      i18n="@@family.accept.decline"
                    >
                      Decline
                    </button>
                    <button
                      (click)="accept()"
                      [disabled]="isProcessing()"
                      class="flex-1 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
                    >
                      {{ isProcessing() ? processingLabel : acceptInvitationLabel }}
                    </button>
                  </div>
                }
              }
            </div>
          </div>
        }
      </div>
    </div>
  `,
})
export class InvitationAcceptComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private invitationService = inject(InvitationService);

  readonly acceptedLabel = $localize`:@@family.accept.accepted:Invitation accepted! Redirecting...`;
  readonly declinedLabel = $localize`:@@family.accept.declined:Invitation declined.`;
  readonly processingLabel = $localize`:@@common.processing:Processing...`;
  readonly acceptInvitationLabel = $localize`:@@family.accept.accept:Accept Invitation`;

  invitation = signal<InvitationDto | null>(null);
  isLoading = signal(true);
  isProcessing = signal(false);
  error = signal<string | null>(null);
  actionResult = signal<'accepted' | 'declined' | null>(null);
  isAuthenticated = this.authService.isAuthenticated;

  private token = '';

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.token = params['token'] || '';

      if (!this.token) {
        this.error.set($localize`:@@family.accept.invalidLink:Invalid invitation link`);
        this.isLoading.set(false);
        return;
      }

      this.loadInvitation();
    });
  }

  private loadInvitation(): void {
    this.invitationService.getInvitationByToken(this.token).subscribe({
      next: (invitation) => {
        if (invitation) {
          this.invitation.set(invitation);
        } else {
          this.error.set($localize`:@@family.accept.notFound:Invitation not found or has expired`);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@family.accept.loadFailed:Failed to load invitation details`);
        this.isLoading.set(false);
      },
    });
  }

  loginToAccept(): void {
    // Save the current URL for post-login redirect
    sessionStorage.setItem('post_login_redirect', `/invitation/accept?token=${this.token}`);
    this.authService.login();
  }

  accept(): void {
    this.isProcessing.set(true);
    this.invitationService.acceptInvitation({ token: this.token }).subscribe({
      next: async (result) => {
        if (result?.success) {
          this.actionResult.set('accepted');
          // Refresh user data so dashboard sees updated familyId
          await this.userService.fetchCurrentUser();
          // Redirect to dashboard after short delay
          setTimeout(() => {
            this.router.navigate(['/dashboard']);
          }, 1500);
        } else {
          this.error.set($localize`:@@family.accept.acceptFailed:Failed to accept invitation`);
        }
        this.isProcessing.set(false);
      },
      error: (err) => {
        const message =
          err?.graphQLErrors?.[0]?.message ||
          err?.message ||
          $localize`:@@family.accept.acceptError:An error occurred while accepting the invitation`;
        this.error.set(message);
        this.isProcessing.set(false);
      },
    });
  }

  decline(): void {
    this.isProcessing.set(true);
    this.invitationService.declineInvitation({ token: this.token }).subscribe({
      next: () => {
        this.actionResult.set('declined');
        this.isProcessing.set(false);
      },
      error: () => {
        this.error.set(
          $localize`:@@family.accept.declineError:An error occurred while declining the invitation`,
        );
        this.isProcessing.set(false);
      },
    });
  }
}
