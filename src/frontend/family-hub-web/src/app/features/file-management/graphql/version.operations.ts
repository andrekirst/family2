import { gql } from 'apollo-angular';

export const GET_FILE_VERSIONS = gql`
  query GetFileVersions($fileId: UUID!, $familyId: UUID!) {
    fileManagement {
      getFileVersions(fileId: $fileId, familyId: $familyId) {
        id
        fileId
        versionNumber
        storageKey
        fileSize
        checksum
        uploadedBy
        isCurrent
        uploadedAt
      }
    }
  }
`;

export const RESTORE_FILE_VERSION = gql`
  mutation RestoreFileVersion($versionId: UUID!, $fileId: UUID!) {
    fileManagement {
      restoreFileVersion(versionId: $versionId, fileId: $fileId) {
        success
        newVersionId
        newVersionNumber
      }
    }
  }
`;
