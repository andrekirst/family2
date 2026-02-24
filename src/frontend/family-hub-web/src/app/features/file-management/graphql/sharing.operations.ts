import { gql } from 'apollo-angular';

export const GET_SHARE_LINKS = gql`
  query GetShareLinks($familyId: UUID!) {
    fileManagement {
      shareLinks(familyId: $familyId) {
        id
        token
        resourceType
        resourceId
        familyId
        createdBy
        expiresAt
        hasPassword
        maxDownloads
        downloadCount
        isRevoked
        isExpired
        isAccessible
        createdAt
      }
    }
  }
`;

export const GET_SHARE_LINK_ACCESS_LOG = gql`
  query GetShareLinkAccessLog($shareLinkId: UUID!, $familyId: UUID!) {
    fileManagement {
      shareLinkAccessLog(shareLinkId: $shareLinkId, familyId: $familyId) {
        id
        shareLinkId
        ipAddress
        userAgent
        action
        accessedAt
      }
    }
  }
`;

export const GET_PERMISSIONS = gql`
  query GetPermissions($resourceType: String!, $resourceId: UUID!) {
    fileManagement {
      permissions(resourceType: $resourceType, resourceId: $resourceId) {
        id
        resourceType
        resourceId
        memberId
        permissionLevel
        grantedBy
        grantedAt
      }
    }
  }
`;

export const CREATE_SHARE_LINK = gql`
  mutation CreateShareLink(
    $resourceType: String!
    $resourceId: UUID!
    $familyId: UUID!
    $expiresAt: DateTime
    $password: String
    $maxDownloads: Int
  ) {
    fileManagement {
      createShareLink(
        resourceType: $resourceType
        resourceId: $resourceId
        familyId: $familyId
        expiresAt: $expiresAt
        password: $password
        maxDownloads: $maxDownloads
      ) {
        success
        shareLinkId
        token
      }
    }
  }
`;

export const REVOKE_SHARE_LINK = gql`
  mutation RevokeShareLink($shareLinkId: UUID!, $familyId: UUID!) {
    fileManagement {
      revokeShareLink(shareLinkId: $shareLinkId, familyId: $familyId) {
        success
      }
    }
  }
`;

export const SET_PERMISSION = gql`
  mutation SetPermission($input: SetPermissionRequestInput!) {
    fileManagement {
      setPermission(input: $input) {
        success
        permissionId
      }
    }
  }
`;

export const REMOVE_PERMISSION = gql`
  mutation RemovePermission($resourceType: String!, $resourceId: UUID!, $memberId: UUID!) {
    fileManagement {
      removePermission(resourceType: $resourceType, resourceId: $resourceId, memberId: $memberId) {
        success
      }
    }
  }
`;
