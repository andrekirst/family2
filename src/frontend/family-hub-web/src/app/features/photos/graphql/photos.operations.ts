import { gql } from 'apollo-angular';

export const GET_PHOTOS = gql`
  query GetPhotos($familyId: UUID!, $skip: Int!, $take: Int!) {
    family {
      photos(familyId: $familyId, skip: $skip, take: $take) {
        items {
          id
          familyId
          uploadedBy
          fileName
          contentType
          fileSizeBytes
          storagePath
          caption
          createdAt
          updatedAt
        }
        totalCount
        hasMore
      }
    }
  }
`;

export const GET_PHOTO = gql`
  query GetPhoto($id: UUID!) {
    family {
      photo(id: $id) {
        id
        familyId
        uploadedBy
        fileName
        contentType
        fileSizeBytes
        storagePath
        caption
        createdAt
        updatedAt
      }
    }
  }
`;

export const GET_ADJACENT_PHOTOS = gql`
  query GetAdjacentPhotos($familyId: UUID!, $currentPhotoId: UUID!, $currentCreatedAt: DateTime!) {
    family {
      adjacentPhotos(
        familyId: $familyId
        currentPhotoId: $currentPhotoId
        currentCreatedAt: $currentCreatedAt
      ) {
        previous {
          id
          familyId
          uploadedBy
          fileName
          contentType
          fileSizeBytes
          storagePath
          caption
          createdAt
          updatedAt
        }
        next {
          id
          familyId
          uploadedBy
          fileName
          contentType
          fileSizeBytes
          storagePath
          caption
          createdAt
          updatedAt
        }
      }
    }
  }
`;

export const UPLOAD_PHOTO = gql`
  mutation UploadPhoto($input: UploadPhotoRequestInput!) {
    family {
      photos {
        upload(input: $input) {
          id
          familyId
          uploadedBy
          fileName
          contentType
          fileSizeBytes
          storagePath
          caption
          createdAt
          updatedAt
        }
      }
    }
  }
`;

export const UPDATE_PHOTO_CAPTION = gql`
  mutation UpdatePhotoCaption($id: UUID!, $input: UpdatePhotoCaptionRequestInput!) {
    family {
      photos {
        updateCaption(id: $id, input: $input) {
          id
          familyId
          uploadedBy
          fileName
          contentType
          fileSizeBytes
          storagePath
          caption
          createdAt
          updatedAt
        }
      }
    }
  }
`;

export const DELETE_PHOTO = gql`
  mutation DeletePhoto($id: UUID!) {
    family {
      photos {
        delete(id: $id)
      }
    }
  }
`;
