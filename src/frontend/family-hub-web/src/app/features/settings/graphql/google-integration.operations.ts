import { gql } from 'apollo-angular';

export const GET_LINKED_ACCOUNTS = gql`
  query GetLinkedAccounts {
    googleIntegration {
      linkedAccounts {
        googleAccountId
        googleEmail
        status
        grantedScopes
        lastSyncAt
        createdAt
      }
    }
  }
`;

export const GET_CALENDAR_SYNC_STATUS = gql`
  query GetCalendarSyncStatus {
    googleIntegration {
      calendarSyncStatus {
        isLinked
        lastSyncAt
        hasCalendarScope
        status
        errorMessage
      }
    }
  }
`;

export const GET_GOOGLE_AUTH_URL = gql`
  query GetGoogleAuthUrl {
    googleIntegration {
      authUrl
    }
  }
`;

export const UNLINK_GOOGLE_ACCOUNT = gql`
  mutation UnlinkGoogleAccount {
    googleIntegration {
      unlink
    }
  }
`;

export const REFRESH_GOOGLE_TOKEN = gql`
  mutation RefreshGoogleToken {
    googleIntegration {
      refreshToken {
        success
        newExpiresAt
      }
    }
  }
`;
