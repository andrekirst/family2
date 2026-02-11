import { gql } from 'apollo-angular';

export const SEND_INVITATION = gql`
  mutation SendInvitation($input: SendInvitationRequestInput!) {
    family {
      invite(input: $input) {
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
  }
`;

export const ACCEPT_INVITATION = gql`
  mutation AcceptInvitation($input: AcceptInvitationRequestInput!) {
    family {
      invitation {
        acceptByToken(input: $input) {
          familyId
          familyMemberId
          success
        }
      }
    }
  }
`;

export const DECLINE_INVITATION = gql`
  mutation DeclineInvitation($input: AcceptInvitationRequestInput!) {
    family {
      invitation {
        declineByToken(input: $input)
      }
    }
  }
`;

export const REVOKE_INVITATION = gql`
  mutation RevokeInvitation($invitationId: UUID!) {
    family {
      invitation(id: $invitationId) {
        revoke
      }
    }
  }
`;

export const GET_PENDING_INVITATIONS = gql`
  query GetPendingInvitations {
    invitations {
      pendings {
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
  }
`;

export const GET_INVITATION_BY_TOKEN = gql`
  query GetInvitationByToken($token: String!) {
    invitations {
      byToken(token: $token) {
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
  }
`;

export const GET_MY_PENDING_INVITATIONS = gql`
  query GetMyPendingInvitations {
    me {
      invitations {
        pendings {
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
    }
  }
`;

export const ACCEPT_INVITATION_BY_ID = gql`
  mutation AcceptInvitationById($invitationId: UUID!) {
    family {
      invitation(id: $invitationId) {
        accept {
          familyId
          familyMemberId
          success
        }
      }
    }
  }
`;

export const DECLINE_INVITATION_BY_ID = gql`
  mutation DeclineInvitationById($invitationId: UUID!) {
    family {
      invitation(id: $invitationId) {
        decline
      }
    }
  }
`;

export const GET_FAMILY_MEMBERS = gql`
  query GetFamilyMembersWithRoles {
    me {
      family {
        withRoles {
          id
          userId
          userName
          userEmail
          role
          joinedAt
          isActive
        }
      }
    }
  }
`;
