import { gql } from 'apollo-angular';

export const GET_ALBUMS = gql`
  query GetAlbums {
    fileManagement {
      getAlbums {
        id
        name
        description
        coverFileId
        familyId
        createdBy
        itemCount
        createdAt
        updatedAt
      }
    }
  }
`;

export const CREATE_ALBUM = gql`
  mutation CreateAlbum($input: CreateAlbumRequestInput!) {
    fileManagement {
      createAlbum(input: $input) {
        albumId
      }
    }
  }
`;

export const RENAME_ALBUM = gql`
  mutation RenameAlbum($input: RenameAlbumRequestInput!) {
    fileManagement {
      renameAlbum(input: $input) {
        albumId
      }
    }
  }
`;

export const DELETE_ALBUM = gql`
  mutation DeleteAlbum($albumId: UUID!) {
    fileManagement {
      deleteAlbum(albumId: $albumId) {
        success
      }
    }
  }
`;

export const ADD_FILE_TO_ALBUM = gql`
  mutation AddFileToAlbum($albumId: UUID!, $fileId: UUID!) {
    fileManagement {
      addFileToAlbum(albumId: $albumId, fileId: $fileId) {
        success
      }
    }
  }
`;

export const REMOVE_FILE_FROM_ALBUM = gql`
  mutation RemoveFileFromAlbum($albumId: UUID!, $fileId: UUID!) {
    fileManagement {
      removeFileFromAlbum(albumId: $albumId, fileId: $fileId) {
        success
      }
    }
  }
`;
