import { gql } from 'apollo-angular';

// Batch operations use existing individual mutations called in parallel.
// These GraphQL operations are placeholders for when the backend adds
// dedicated batch endpoints (BatchOperationType enum already exists).

export const MOVE_FILE = gql`
  mutation MoveFile($input: MoveFileRequestInput!) {
    fileManagement {
      moveFile(input: $input) {
        id
        name
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
