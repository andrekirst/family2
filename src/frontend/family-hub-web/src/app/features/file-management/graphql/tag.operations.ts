import { gql } from 'apollo-angular';

const TAG_FIELDS = `
  id
  name
  color
  familyId
  fileCount
  createdAt
`;

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

export const GET_TAGS = gql`
  query GetTags {
    fileManagement {
      getTags {
        ${TAG_FIELDS}
      }
    }
  }
`;

export const GET_FILES_BY_TAG = gql`
  query GetFilesByTag($tagId: UUID!) {
    fileManagement {
      getFilesByTag(tagId: $tagId) {
        ${FILE_FIELDS}
      }
    }
  }
`;

export const CREATE_TAG = gql`
  mutation CreateTag($input: CreateTagRequestInput!) {
    fileManagement {
      createTag(input: $input) {
        ${TAG_FIELDS}
      }
    }
  }
`;

export const UPDATE_TAG = gql`
  mutation UpdateTag($input: UpdateTagRequestInput!) {
    fileManagement {
      updateTag(input: $input) {
        ${TAG_FIELDS}
      }
    }
  }
`;

export const DELETE_TAG = gql`
  mutation DeleteTag($tagId: UUID!) {
    fileManagement {
      deleteTag(tagId: $tagId)
    }
  }
`;

export const TAG_FILE = gql`
  mutation TagFile($fileId: UUID!, $tagId: UUID!) {
    fileManagement {
      tagFile(fileId: $fileId, tagId: $tagId) {
        ${FILE_FIELDS}
      }
    }
  }
`;

export const UNTAG_FILE = gql`
  mutation UntagFile($fileId: UUID!, $tagId: UUID!) {
    fileManagement {
      untagFile(fileId: $fileId, tagId: $tagId) {
        ${FILE_FIELDS}
      }
    }
  }
`;
