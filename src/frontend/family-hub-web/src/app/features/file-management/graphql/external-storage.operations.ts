import { gql } from 'apollo-angular';

export const GET_EXTERNAL_CONNECTIONS = gql`
  query GetExternalConnections {
    fileManagement {
      externalConnections {
        id
        familyId
        providerType
        displayName
        status
        isTokenExpired
        tokenExpiresAt
        connectedBy
        connectedAt
      }
    }
  }
`;

export const CONNECT_EXTERNAL_STORAGE = gql`
  mutation ConnectExternalStorage(
    $providerType: String!
    $displayName: String!
    $encryptedAccessToken: String!
    $encryptedRefreshToken: String
    $tokenExpiresAt: DateTime
  ) {
    fileManagement {
      connectExternalStorage(
        providerType: $providerType
        displayName: $displayName
        encryptedAccessToken: $encryptedAccessToken
        encryptedRefreshToken: $encryptedRefreshToken
        tokenExpiresAt: $tokenExpiresAt
      ) {
        connectionId
      }
    }
  }
`;

export const DISCONNECT_EXTERNAL_STORAGE = gql`
  mutation DisconnectExternalStorage($connectionId: UUID!) {
    fileManagement {
      disconnectExternalStorage(connectionId: $connectionId) {
        success
      }
    }
  }
`;
