import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import {
  SEND_INVITATION,
  ACCEPT_INVITATION,
  DECLINE_INVITATION,
  REVOKE_INVITATION,
  GET_PENDING_INVITATIONS,
  GET_INVITATION_BY_TOKEN,
  GET_FAMILY_MEMBERS,
  GET_MY_PENDING_INVITATIONS,
  ACCEPT_INVITATION_BY_ID,
  DECLINE_INVITATION_BY_ID,
} from '../graphql/invitation.operations';
import {
  InvitationDto,
  FamilyMemberDto,
  AcceptInvitationResult,
  SendInvitationInput,
  AcceptInvitationInput,
} from '../models/invitation.models';
import { catchError, map, of, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InvitationService {
  private apollo = inject(Apollo);

  sendInvitation(input: SendInvitationInput) {
    return this.apollo
      .mutate<{ sendInvitation: InvitationDto }>({
        mutation: SEND_INVITATION,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.sendInvitation),
        catchError((error) => {
          console.error('Failed to send invitation:', error);
          return of(null);
        }),
      );
  }

  acceptInvitation(input: AcceptInvitationInput) {
    return this.apollo
      .mutate<{ acceptInvitation: AcceptInvitationResult }>({
        mutation: ACCEPT_INVITATION,
        variables: { input },
        refetchQueries: ['GetCurrentUser'],
      })
      .pipe(
        map((result) => result.data?.acceptInvitation),
        catchError((error) => {
          console.error('Failed to accept invitation:', error);
          return throwError(() => error);
        }),
      );
  }

  declineInvitation(input: AcceptInvitationInput) {
    return this.apollo
      .mutate<{ declineInvitation: boolean }>({
        mutation: DECLINE_INVITATION,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.declineInvitation),
        catchError((error) => {
          console.error('Failed to decline invitation:', error);
          return of(false);
        }),
      );
  }

  revokeInvitation(invitationId: string) {
    return this.apollo
      .mutate<{ revokeInvitation: boolean }>({
        mutation: REVOKE_INVITATION,
        variables: { invitationId },
      })
      .pipe(
        map((result) => result.data?.revokeInvitation),
        catchError((error) => {
          console.error('Failed to revoke invitation:', error);
          return of(false);
        }),
      );
  }

  getPendingInvitations() {
    return this.apollo
      .query<{ pendingInvitations: InvitationDto[] }>({
        query: GET_PENDING_INVITATIONS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.pendingInvitations ?? []),
        catchError((error) => {
          console.error('Failed to get pending invitations:', error);
          return of([] as InvitationDto[]);
        }),
      );
  }

  getInvitationByToken(token: string) {
    return this.apollo
      .query<{ invitationByToken: InvitationDto | null }>({
        query: GET_INVITATION_BY_TOKEN,
        variables: { token },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.invitationByToken),
        catchError((error) => {
          console.error('Failed to get invitation:', error);
          return of(null);
        }),
      );
  }

  getMyPendingInvitations() {
    return this.apollo
      .query<{ myPendingInvitations: InvitationDto[] }>({
        query: GET_MY_PENDING_INVITATIONS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.myPendingInvitations ?? []),
        catchError((error) => {
          console.error('Failed to get my pending invitations:', error);
          return of([] as InvitationDto[]);
        }),
      );
  }

  acceptInvitationById(invitationId: string) {
    return this.apollo
      .mutate<{ acceptInvitationById: AcceptInvitationResult }>({
        mutation: ACCEPT_INVITATION_BY_ID,
        variables: { invitationId },
        refetchQueries: ['GetCurrentUser'],
      })
      .pipe(
        map((result) => result.data?.acceptInvitationById),
        catchError((error) => {
          console.error('Failed to accept invitation by ID:', error);
          return throwError(() => error);
        }),
      );
  }

  declineInvitationById(invitationId: string) {
    return this.apollo
      .mutate<{ declineInvitationById: boolean }>({
        mutation: DECLINE_INVITATION_BY_ID,
        variables: { invitationId },
      })
      .pipe(
        map((result) => result.data?.declineInvitationById),
        catchError((error) => {
          console.error('Failed to decline invitation by ID:', error);
          return throwError(() => error);
        }),
      );
  }

  getFamilyMembers() {
    return this.apollo
      .query<{ familyMembersWithRoles: FamilyMemberDto[] }>({
        query: GET_FAMILY_MEMBERS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.familyMembersWithRoles ?? []),
        catchError((error) => {
          console.error('Failed to get family members:', error);
          return of([] as FamilyMemberDto[]);
        }),
      );
  }
}
