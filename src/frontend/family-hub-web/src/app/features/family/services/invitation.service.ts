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
      .mutate<{ family: { invite: InvitationDto } }>({
        mutation: SEND_INVITATION,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.family?.invite),
        catchError((error) => {
          console.error('Failed to send invitation:', error);
          return of(null);
        }),
      );
  }

  acceptInvitation(input: AcceptInvitationInput) {
    return this.apollo
      .mutate<{ family: { invitation: { acceptByToken: AcceptInvitationResult } } }>({
        mutation: ACCEPT_INVITATION,
        variables: { input },
        refetchQueries: ['GetMyProfile'],
      })
      .pipe(
        map((result) => result.data?.family?.invitation?.acceptByToken),
        catchError((error) => {
          console.error('Failed to accept invitation:', error);
          return throwError(() => error);
        }),
      );
  }

  declineInvitation(input: AcceptInvitationInput) {
    return this.apollo
      .mutate<{ family: { invitation: { declineByToken: boolean } } }>({
        mutation: DECLINE_INVITATION,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.family?.invitation?.declineByToken),
        catchError((error) => {
          console.error('Failed to decline invitation:', error);
          return of(false);
        }),
      );
  }

  revokeInvitation(invitationId: string) {
    return this.apollo
      .mutate<{ family: { invitation: { revoke: boolean } } }>({
        mutation: REVOKE_INVITATION,
        variables: { invitationId },
      })
      .pipe(
        map((result) => result.data?.family?.invitation?.revoke),
        catchError((error) => {
          console.error('Failed to revoke invitation:', error);
          return of(false);
        }),
      );
  }

  getPendingInvitations() {
    return this.apollo
      .query<{ invitations: { pendings: InvitationDto[] } }>({
        query: GET_PENDING_INVITATIONS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.invitations?.pendings ?? []),
        catchError((error) => {
          console.error('Failed to get pending invitations:', error);
          return of([] as InvitationDto[]);
        }),
      );
  }

  getInvitationByToken(token: string) {
    return this.apollo
      .query<{ invitations: { byToken: InvitationDto | null } }>({
        query: GET_INVITATION_BY_TOKEN,
        variables: { token },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.invitations?.byToken),
        catchError((error) => {
          console.error('Failed to get invitation:', error);
          return of(null);
        }),
      );
  }

  getMyPendingInvitations() {
    return this.apollo
      .query<{ me: { invitations: { pendings: InvitationDto[] } } }>({
        query: GET_MY_PENDING_INVITATIONS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.me?.invitations?.pendings ?? []),
        catchError((error) => {
          console.error('Failed to get my pending invitations:', error);
          return of([] as InvitationDto[]);
        }),
      );
  }

  acceptInvitationById(invitationId: string) {
    return this.apollo
      .mutate<{ family: { invitation: { accept: AcceptInvitationResult } } }>({
        mutation: ACCEPT_INVITATION_BY_ID,
        variables: { invitationId },
        refetchQueries: ['GetMyProfile'],
      })
      .pipe(
        map((result) => result.data?.family?.invitation?.accept),
        catchError((error) => {
          console.error('Failed to accept invitation by ID:', error);
          return throwError(() => error);
        }),
      );
  }

  declineInvitationById(invitationId: string) {
    return this.apollo
      .mutate<{ family: { invitation: { decline: boolean } } }>({
        mutation: DECLINE_INVITATION_BY_ID,
        variables: { invitationId },
      })
      .pipe(
        map((result) => result.data?.family?.invitation?.decline),
        catchError((error) => {
          console.error('Failed to decline invitation by ID:', error);
          return throwError(() => error);
        }),
      );
  }

  getFamilyMembers() {
    return this.apollo
      .query<{ me: { family: { withRoles: FamilyMemberDto[] } } }>({
        query: GET_FAMILY_MEMBERS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.me?.family?.withRoles ?? []),
        catchError((error) => {
          console.error('Failed to get family members:', error);
          return of([] as FamilyMemberDto[]);
        }),
      );
  }
}
