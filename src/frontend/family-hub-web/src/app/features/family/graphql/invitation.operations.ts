import { gql } from 'apollo-angular';

export const SEND_INVITATION = gql`
  mutation SendInvitation($input: SendInvitationRequestInput!) {
    sendInvitation(input: $input) {
      id
      familyId
      familyName
      invitedByName
      inviteeEmail
      role
      status
      createdAt
      expiresAt
    }
  }
`;

export const ACCEPT_INVITATION = gql`
  mutation AcceptInvitation($input: AcceptInvitationRequestInput!) {
    acceptInvitation(input: $input) {
      familyId
      familyMemberId
      success
    }
  }
`;

export const DECLINE_INVITATION = gql`
  mutation DeclineInvitation($input: AcceptInvitationRequestInput!) {
    declineInvitation(input: $input)
  }
`;

export const REVOKE_INVITATION = gql`
  mutation RevokeInvitation($invitationId: UUID!) {
    revokeInvitation(invitationId: $invitationId)
  }
`;

export const GET_PENDING_INVITATIONS = gql`
  query GetPendingInvitations {
    pendingInvitations {
      id
      familyId
      familyName
      invitedByName
      inviteeEmail
      role
      status
      createdAt
      expiresAt
    }
  }
`;

export const GET_INVITATION_BY_TOKEN = gql`
  query GetInvitationByToken($token: String!) {
    invitationByToken(token: $token) {
      id
      familyId
      familyName
      invitedByName
      inviteeEmail
      role
      status
      createdAt
      expiresAt
    }
  }
`;

export const GET_FAMILY_MEMBERS = gql`
  query GetFamilyMembersWithRoles {
    familyMembersWithRoles {
      id
      userId
      userName
      userEmail
      role
      joinedAt
      isActive
    }
  }
`;
