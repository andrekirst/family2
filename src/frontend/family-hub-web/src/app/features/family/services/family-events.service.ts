import { Injectable, inject, signal } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { Subscription } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';

/**
 * Change type enum for subscription events
 */
export type ChangeType = 'ADDED' | 'UPDATED' | 'REMOVED';

/**
 * User role enum
 */
export type UserRole = 'OWNER' | 'ADMIN' | 'MEMBER' | 'CHILD';

/**
 * Family member type
 */
export interface FamilyMember {
  id: string;
  email: string;
  role: UserRole;
  joinedAt: string;
  emailVerified: boolean;
  isOwner: boolean;
}

/**
 * Pending invitation type
 */
export interface PendingInvitation {
  id: string;
  email: string;
  role: UserRole;
  status: string;
  invitedById: string;
  invitedAt: string;
  expiresAt: string;
  message?: string;
  displayCode: string;
}

/**
 * Family members changed payload
 */
export interface FamilyMembersChangedPayload {
  familyId: string;
  changeType: ChangeType;
  member: FamilyMember;
}

/**
 * Pending invitations changed payload
 */
export interface PendingInvitationsChangedPayload {
  familyId: string;
  changeType: ChangeType;
  invitation: PendingInvitation;
}

/**
 * Family Events Service - Real-time subscription updates
 *
 * **Purpose:** Provides real-time updates for family members and invitations via GraphQL subscriptions
 *
 * **Features:**
 * - Signal-based reactive state for subscription events
 * - WebSocket connection status tracking
 * - Auto-cleanup on unsubscribe
 * - Toast notifications for events
 * - Support for multiple subscription types
 *
 * **Subscriptions:**
 * 1. familyMembersChanged - Fires when member ADDED/UPDATED/REMOVED
 * 2. pendingInvitationsChanged - Fires when invitation ADDED/UPDATED/REMOVED (requires OWNER/ADMIN)
 *
 * **Usage:**
 * ```typescript
 * private familyEventsService = inject(FamilyEventsService);
 *
 * ngOnInit() {
 *   // Subscribe to family member changes
 *   this.familyEventsService.subscribeFamilyMembers('family-id');
 *
 *   // React to events
 *   effect(() => {
 *     const event = this.familyEventsService.lastMemberEvent();
 *     if (event?.changeType === 'ADDED') {
 *       // Refresh member list or update UI
 *     }
 *   });
 * }
 *
 * ngOnDestroy() {
 *   this.familyEventsService.unsubscribeAll();
 * }
 * ```
 *
 * @example
 * ```typescript
 * // In dashboard component
 * export class DashboardComponent implements OnInit, OnDestroy {
 *   private familyEventsService = inject(FamilyEventsService);
 *
 *   ngOnInit() {
 *     const familyId = this.familyService.currentFamily()?.id;
 *     if (familyId) {
 *       this.familyEventsService.subscribeFamilyMembers(familyId);
 *     }
 *
 *     effect(() => {
 *       const event = this.familyEventsService.lastMemberEvent();
 *       if (event) {
 *         console.log('Member event:', event.changeType, event.member.email);
 *       }
 *     });
 *   }
 *
 *   ngOnDestroy() {
 *     this.familyEventsService.unsubscribeAll();
 *   }
 * }
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class FamilyEventsService {
  private apollo = inject(Apollo);
  private toastService = inject(ToastService);

  /**
   * Signal for last family member event
   */
  private lastMemberEventSignal = signal<FamilyMembersChangedPayload | null>(null);
  public lastMemberEvent = this.lastMemberEventSignal.asReadonly();

  /**
   * Signal for last invitation event
   */
  private lastInvitationEventSignal = signal<PendingInvitationsChangedPayload | null>(null);
  public lastInvitationEvent = this.lastInvitationEventSignal.asReadonly();

  /**
   * Signal for connection status
   */
  private isConnectedSignal = signal(false);
  public isConnected = this.isConnectedSignal.asReadonly();

  /**
   * Signal for connection error
   */
  private connectionErrorSignal = signal<string | null>(null);
  public connectionError = this.connectionErrorSignal.asReadonly();

  /**
   * Active subscriptions map (for cleanup)
   */
  private subscriptions = new Map<string, Subscription>();

  /**
   * GraphQL subscription query for family members changes
   */
  private readonly FAMILY_MEMBERS_CHANGED = gql`
    subscription FamilyMembersChanged($familyId: ID!) {
      familyMembersChanged(familyId: $familyId) {
        familyId
        changeType
        member {
          id
          email
          role
          joinedAt
          emailVerified
          isOwner
        }
      }
    }
  `;

  /**
   * GraphQL subscription query for pending invitations changes
   */
  private readonly PENDING_INVITATIONS_CHANGED = gql`
    subscription PendingInvitationsChanged($familyId: ID!) {
      pendingInvitationsChanged(familyId: $familyId) {
        familyId
        changeType
        invitation {
          id
          email
          role
          status
          invitedById
          invitedAt
          expiresAt
          message
          displayCode
        }
      }
    }
  `;

  /**
   * Subscribes to family members changes
   * Fires when members are ADDED, UPDATED, or REMOVED
   *
   * @param familyId - Family ID to subscribe to
   */
  subscribeFamilyMembers(familyId: string): void {
    // Unsubscribe existing subscription for this family
    this.unsubscribeFamilyMembers();

    const subscription = this.apollo
      .subscribe<{ familyMembersChanged: FamilyMembersChangedPayload }>({
        query: this.FAMILY_MEMBERS_CHANGED,
        variables: { familyId },
      })
      .subscribe({
        next: (result) => {
          if (result.data) {
            const event = result.data.familyMembersChanged;
            this.lastMemberEventSignal.set(event);
            this.isConnectedSignal.set(true);
            this.connectionErrorSignal.set(null);

            // Show toast notification
            if (event.changeType === 'ADDED') {
              this.toastService.info(`${event.member.email} joined the family`);
            } else if (event.changeType === 'REMOVED') {
              this.toastService.info(`${event.member.email} left the family`);
            }
          }
        },
        error: (error) => {
          console.error('[FamilyEventsService] familyMembersChanged error:', error);
          this.isConnectedSignal.set(false);
          this.connectionErrorSignal.set(error.message || 'Subscription error');
          this.toastService.error('Lost connection to real-time updates');
        },
        complete: () => {
          console.log('[FamilyEventsService] familyMembersChanged complete');
          this.isConnectedSignal.set(false);
        },
      });

    this.subscriptions.set('familyMembers', subscription);
  }

  /**
   * Subscribes to pending invitations changes
   * Fires when invitations are ADDED, UPDATED, or REMOVED
   * Requires OWNER or ADMIN role (backend enforces authorization)
   *
   * @param familyId - Family ID to subscribe to
   */
  subscribePendingInvitations(familyId: string): void {
    // Unsubscribe existing subscription for this family
    this.unsubscribePendingInvitations();

    const subscription = this.apollo
      .subscribe<{ pendingInvitationsChanged: PendingInvitationsChangedPayload }>({
        query: this.PENDING_INVITATIONS_CHANGED,
        variables: { familyId },
      })
      .subscribe({
        next: (result) => {
          if (result.data) {
            const event = result.data.pendingInvitationsChanged;
            this.lastInvitationEventSignal.set(event);
            this.isConnectedSignal.set(true);
            this.connectionErrorSignal.set(null);

            // Show toast notification
            if (event.changeType === 'ADDED') {
              this.toastService.info(`Invitation sent to ${event.invitation.email}`);
            } else if (event.changeType === 'REMOVED') {
              this.toastService.info(`Invitation for ${event.invitation.email} was removed`);
            }
          }
        },
        error: (error) => {
          console.error('[FamilyEventsService] pendingInvitationsChanged error:', error);
          this.isConnectedSignal.set(false);
          this.connectionErrorSignal.set(error.message || 'Subscription error');
          this.toastService.error('Lost connection to real-time updates');
        },
        complete: () => {
          console.log('[FamilyEventsService] pendingInvitationsChanged complete');
          this.isConnectedSignal.set(false);
        },
      });

    this.subscriptions.set('pendingInvitations', subscription);
  }

  /**
   * Unsubscribes from family members changes
   */
  unsubscribeFamilyMembers(): void {
    const subscription = this.subscriptions.get('familyMembers');
    if (subscription) {
      subscription.unsubscribe();
      this.subscriptions.delete('familyMembers');
      this.lastMemberEventSignal.set(null);
    }
  }

  /**
   * Unsubscribes from pending invitations changes
   */
  unsubscribePendingInvitations(): void {
    const subscription = this.subscriptions.get('pendingInvitations');
    if (subscription) {
      subscription.unsubscribe();
      this.subscriptions.delete('pendingInvitations');
      this.lastInvitationEventSignal.set(null);
    }
  }

  /**
   * Unsubscribes from all active subscriptions
   * Call this in ngOnDestroy to prevent memory leaks
   */
  unsubscribeAll(): void {
    this.subscriptions.forEach((subscription) => {
      subscription.unsubscribe();
    });
    this.subscriptions.clear();
    this.lastMemberEventSignal.set(null);
    this.lastInvitationEventSignal.set(null);
    this.isConnectedSignal.set(false);
    this.connectionErrorSignal.set(null);
  }
}
