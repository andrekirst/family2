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

export const GET_FAVORITES = gql`
  query GetFavorites {
    fileManagement {
      getFavorites {
        ${FILE_FIELDS}
      }
    }
  }
`;

export const TOGGLE_FAVORITE = gql`
  mutation ToggleFavorite($fileId: UUID!) {
    fileManagement {
      toggleFavorite(fileId: $fileId) {
        ${FILE_FIELDS}
      }
    }
  }
`;
