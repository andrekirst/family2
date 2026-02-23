import { gql } from 'apollo-angular';

export const GET_MEDIA_STREAM_INFO = gql`
  query GetMediaStreamInfo($fileId: UUID!, $familyId: UUID!) {
    fileManagement {
      getMediaStreamInfo(fileId: $fileId, familyId: $familyId) {
        fileId
        mimeType
        fileSize
        storageKey
        supportsRangeRequests
        isStreamable
        thumbnails {
          id
          fileId
          width
          height
          storageKey
          generatedAt
        }
      }
    }
  }
`;

export const GENERATE_THUMBNAILS = gql`
  mutation GenerateThumbnails($fileId: UUID!, $familyId: UUID!) {
    fileManagement {
      generateThumbnails(fileId: $fileId, familyId: $familyId) {
        success
        thumbnailsGenerated
      }
    }
  }
`;
