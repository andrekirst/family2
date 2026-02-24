import { gql } from 'apollo-angular';

const FILE_FIELDS = `
  id
  name
  mimeType
  size
  storageKey
  checksum
  folderId
  familyId
  uploadedBy
  createdAt
  updatedAt
`;

export const GET_FILES = gql`
  query GetFiles($folderId: UUID) {
    fileManagement {
      files(folderId: $folderId) {
        ${FILE_FIELDS}
      }
    }
  }
`;

export const GET_FILE = gql`
  query GetFile($fileId: UUID!) {
    fileManagement {
      file(fileId: $fileId) {
        ${FILE_FIELDS}
      }
    }
  }
`;

export const UPLOAD_FILE = gql`
  mutation UploadFile($input: UploadFileRequestInput!) {
    fileManagement {
      uploadFile(input: $input) {
        ${FILE_FIELDS}
      }
    }
  }
`;

export const RENAME_FILE = gql`
  mutation RenameFile($input: RenameFileRequestInput!) {
    fileManagement {
      renameFile(input: $input) {
        ${FILE_FIELDS}
      }
    }
  }
`;

export const MOVE_FILE = gql`
  mutation MoveFile($input: MoveFileRequestInput!) {
    fileManagement {
      moveFile(input: $input) {
        ${FILE_FIELDS}
      }
    }
  }
`;

export const DELETE_FILE = gql`
  mutation DeleteFile($fileId: UUID!) {
    fileManagement {
      deleteFile(fileId: $fileId)
    }
  }
`;
