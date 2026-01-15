import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { GraphQLService } from '../../../../core/services/graphql.service';
import { ToastService } from '../../../../core/services/toast.service';
import { SpinnerComponent } from '../../../../shared/components/atoms/spinner/spinner.component';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';

interface InvitationDetails {
  id: string;
  email: string;
  role: 'ADMIN' | 'MEMBER';
  status: 'PENDING' | 'ACCEPTED' | 'EXPIRED' | 'REVOKED';
  expiresAt: string;
  message: string | null;
  displayCode: string;
  family: {
    id: string;
    name: string;
  };
  memberCount: number;
}

@Component({
  selector: 'app-accept-invitation',
  imports: [SpinnerComponent, ButtonComponent],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4 py-8">
      <div class="max-w-2xl w-full">
        @if (isLoading()) {
          <!-- Loading State -->
          <div class="text-center">
            <app-spinner size="lg"></app-spinner>
            <p class="mt-4 text-gray-600">Loading invitation details...</p>
          </div>
        }

        @if (!isLoading() && error()) {
          <!-- Error State -->
          <div class="bg-white shadow-lg rounded-lg p-8">
            <div class="text-center">
              <div
                class="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-red-100"
              >
                <svg
                  class="h-10 w-10 text-red-600"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </div>
              <h2 class="mt-6 text-2xl font-bold text-gray-900">Invitation Error</h2>
              <p class="mt-2 text-red-600">{{ error() }}</p>
              <div class="mt-8">
                <app-button variant="primary" (clicked)="navigateToLogin()">
                  Back to Login
                </app-button>
              </div>
            </div>
          </div>
        }

        @if (!isLoading() && !error() && invitation()) {
          <!-- Success State - Valid Invitation -->
          <div class="bg-white shadow-lg rounded-lg overflow-hidden">
            <!-- Header with Gradient -->
            <div class="bg-gradient-to-r from-blue-500 to-blue-600 px-8 py-10 text-center">
              <h1 class="text-3xl font-bold text-white mb-2">You're Invited!</h1>
              <p class="text-blue-100 text-lg">Join {{ invitation()!.family.name }}</p>
            </div>

            <!-- Warning Banners -->
            @if (isExpired()) {
              <div class="bg-red-50 border-l-4 border-red-500 p-4">
                <div class="flex items-start">
                  <svg class="h-5 w-5 text-red-500 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                    <path
                      fill-rule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                      clip-rule="evenodd"
                    />
                  </svg>
                  <p class="ml-3 text-sm text-red-700">
                    <strong>This invitation has expired.</strong> Please contact the family owner
                    for a new invitation.
                  </p>
                </div>
              </div>
            }

            @if (isAlreadyAccepted()) {
              <div class="bg-yellow-50 border-l-4 border-yellow-400 p-4">
                <div class="flex items-start">
                  <svg
                    class="h-5 w-5 text-yellow-400 mt-0.5"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path
                      fill-rule="evenodd"
                      d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                      clip-rule="evenodd"
                    />
                  </svg>
                  <p class="ml-3 text-sm text-yellow-700">
                    <strong>This invitation has already been accepted.</strong>
                  </p>
                </div>
              </div>
            }

            <!-- Invitation Details -->
            <div class="px-8 py-6 space-y-6">
              <!-- Family Info Card -->
              <div class="bg-gray-50 rounded-lg p-6">
                <h3 class="text-sm font-medium text-gray-500 uppercase tracking-wide mb-4">
                  Family Details
                </h3>
                <div class="space-y-3">
                  <div class="flex items-center justify-between">
                    <span class="text-gray-700">Family Name:</span>
                    <span class="font-semibold text-gray-900">{{ invitation()!.family.name }}</span>
                  </div>
                  <div class="flex items-center justify-between">
                    <span class="text-gray-700">Current Members:</span>
                    <span
                      class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800"
                    >
                      {{ invitation()!.memberCount }}
                      {{ invitation()!.memberCount === 1 ? 'member' : 'members' }}
                    </span>
                  </div>
                  <div class="flex items-center justify-between">
                    <span class="text-gray-700">Your Role:</span>
                    <span
                      class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium {{
                        invitation()!.role === 'ADMIN'
                          ? 'bg-purple-100 text-purple-800'
                          : 'bg-green-100 text-green-800'
                      }}"
                    >
                      {{ invitation()!.role === 'ADMIN' ? 'Administrator' : 'Member' }}
                    </span>
                  </div>
                  <div class="flex items-center justify-between">
                    <span class="text-gray-700">Invitation Code:</span>
                    <span class="font-mono text-sm bg-gray-200 px-2 py-1 rounded">{{
                      invitation()!.displayCode
                    }}</span>
                  </div>
                </div>
              </div>

              <!-- Personal Message (if exists) -->
              @if (invitation()!.message) {
                <div class="border-l-4 border-blue-400 bg-blue-50 p-4 rounded-r-lg">
                  <p class="text-sm font-medium text-blue-800 mb-1">Personal Message:</p>
                  <p class="text-gray-700 italic">"{{ invitation()!.message }}"</p>
                </div>
              }

              <!-- Role Description -->
              <div class="bg-blue-50 rounded-lg p-4">
                <p class="text-sm text-gray-700">
                  @if (invitation()!.role === 'ADMIN') {
                    <strong>As an Administrator</strong>, you'll be able to manage family members,
                    send invitations, and have full access to all family features.
                  } @else {
                    <strong>As a Member</strong>, you'll have access to family features and can
                    collaborate with other family members.
                  }
                </p>
              </div>

              <!-- Action Buttons -->
              <div class="flex items-center justify-end gap-4 pt-4">
                <app-button
                  variant="secondary"
                  (clicked)="navigateToLogin()"
                  [disabled]="isAccepting()"
                >
                  Cancel
                </app-button>
                <app-button
                  variant="primary"
                  (clicked)="acceptInvitation()"
                  [disabled]="!canAccept()"
                  [loading]="isAccepting()"
                >
                  {{ isAccepting() ? 'Accepting...' : 'Accept Invitation' }}
                </app-button>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
})
export class AcceptInvitationComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly graphqlService = inject(GraphQLService);
  private readonly toastService = inject(ToastService);

  // Signals for reactive state
  invitation = signal<InvitationDetails | null>(null);
  isLoading = signal(true);
  isAccepting = signal(false);
  error = signal<string | null>(null);

  // Computed properties
  isExpired = computed(() => {
    const inv = this.invitation();
    return inv ? new Date(inv.expiresAt) < new Date() : false;
  });

  isAlreadyAccepted = computed(() => {
    return this.invitation()?.status === 'ACCEPTED';
  });

  canAccept = computed(() => {
    const inv = this.invitation();
    return inv && inv.status === 'PENDING' && !this.isExpired() && !this.isAccepting();
  });

  async ngOnInit(): Promise<void> {
    // Extract token from query params
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.error.set('Missing invitation token');
      this.isLoading.set(false);
      return;
    }

    await this.loadInvitation(token);
  }

  /**
   * Loads invitation details using the token (public query - no auth required).
   */
  private async loadInvitation(token: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const query = `
        query GetInvitationByToken($token: String!) {
          invitations {
            byToken(token: $token) {
              id
              email
              role
              status
              expiresAt
              message
              displayCode
              family {
                id
                name
              }
              memberCount
            }
          }
        }
      `;

      const response = await this.graphqlService.query<{
        invitations: {
          byToken: InvitationDetails | null;
        };
      }>(query, { token });

      if (!response.invitations.byToken) {
        this.error.set('Invitation not found or has been revoked');
      } else {
        this.invitation.set(response.invitations.byToken);
      }
    } catch (err) {
      console.error('Failed to load invitation:', err);
      this.error.set('Failed to load invitation details. Please try again.');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Handles invitation acceptance.
   * If authenticated: accepts immediately
   * If not authenticated: stores token and redirects to login
   */
  async acceptInvitation(): Promise<void> {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      return;
    }

    // Check if user is authenticated
    if (this.authService.isAuthenticated()) {
      // User is authenticated - accept immediately
      await this.performAcceptMutation(token);
    } else {
      // User is NOT authenticated - store token and redirect to login
      sessionStorage.setItem('pending_invitation_token', token);
      this.toastService.success('Please sign in to accept this invitation');
      this.router.navigate(['/login']);
    }
  }

  /**
   * Performs the acceptance mutation (requires authentication).
   */
  private async performAcceptMutation(token: string): Promise<void> {
    this.isAccepting.set(true);

    try {
      const mutation = `
        mutation AcceptInvitation($input: AcceptInvitationInput!) {
          acceptInvitation(input: $input) {
            familyId
            familyName
            role
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
              ... on BusinessError {
                message
                code
              }
              ... on ValueObjectError {
                message
              }
              ... on UnauthorizedError {
                message
              }
            }
          }
        }
      `;

      const response = await this.graphqlService.mutate<{
        acceptInvitation: {
          familyId: string;
          familyName: string;
          role: string;
          errors: { __typename: string; message: string }[];
        };
      }>(mutation, {
        input: { token },
      });

      // Check for errors
      if (response.acceptInvitation.errors?.length > 0) {
        const errorMessage = response.acceptInvitation.errors[0].message;
        this.toastService.error(errorMessage);
        this.error.set(errorMessage);
        return;
      }

      // Success!
      this.toastService.success(`Welcome to ${response.acceptInvitation.familyName}!`);
      this.router.navigate(['/dashboard']);
    } catch (err) {
      console.error('Failed to accept invitation:', err);
      const errorMessage = err instanceof Error ? err.message : 'Failed to accept invitation';
      this.toastService.error(errorMessage);
      this.error.set(errorMessage);
    } finally {
      this.isAccepting.set(false);
    }
  }

  /**
   * Navigates to the login page.
   */
  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }
}
