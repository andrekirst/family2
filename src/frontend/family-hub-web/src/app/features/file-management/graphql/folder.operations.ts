import { gql } from 'apollo-angular';

const FOLDER_FIELDS = `
  id
  name
  parentFolderId
  materializedPath
  familyId
  createdBy
  createdAt
`;

export const GET_FOLDERS = gql`
  query GetFolders($parentFolderId: UUID) {
    fileManagement {
      folders(parentFolderId: $parentFolderId) {
        ${FOLDER_FIELDS}
      }
    }
  }
`;

export const GET_FOLDER = gql`
  query GetFolder($folderId: UUID!) {
    fileManagement {
      folder(folderId: $folderId) {
        ${FOLDER_FIELDS}
      }
    }
  }
`;

export const GET_BREADCRUMB = gql`
  query GetBreadcrumb($folderId: UUID!) {
    fileManagement {
      breadcrumb(folderId: $folderId) {
        ${FOLDER_FIELDS}
      }
    }
  }
`;

export const CREATE_FOLDER = gql`
  mutation CreateFolder($input: CreateFolderRequestInput!) {
    fileManagement {
      createFolder(input: $input) {
        ${FOLDER_FIELDS}
      }
    }
  }
`;

export const RENAME_FOLDER = gql`
  mutation RenameFolder($input: RenameFolderRequestInput!) {
    fileManagement {
      renameFolder(input: $input) {
        ${FOLDER_FIELDS}
      }
    }
  }
`;

export const MOVE_FOLDER = gql`
  mutation MoveFolder($input: MoveFolderRequestInput!) {
    fileManagement {
      moveFolder(input: $input) {
        ${FOLDER_FIELDS}
      }
    }
  }
`;

export const DELETE_FOLDER = gql`
  mutation DeleteFolder($folderId: UUID!) {
    fileManagement {
      deleteFolder(folderId: $folderId)
    }
  }
`;
