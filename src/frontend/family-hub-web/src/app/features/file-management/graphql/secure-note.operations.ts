import { gql } from 'apollo-angular';

export const GET_SECURE_NOTES = gql`
  query GetSecureNotes($category: String) {
    fileManagement {
      getSecureNotes(category: $category) {
        id
        familyId
        userId
        category
        encryptedTitle
        encryptedContent
        iv
        salt
        sentinel
        createdAt
        updatedAt
      }
    }
  }
`;

export const CREATE_SECURE_NOTE = gql`
  mutation CreateSecureNote(
    $category: String!
    $encryptedTitle: String!
    $encryptedContent: String!
    $iv: String!
    $salt: String!
    $sentinel: String!
  ) {
    fileManagement {
      createSecureNote(
        category: $category
        encryptedTitle: $encryptedTitle
        encryptedContent: $encryptedContent
        iv: $iv
        salt: $salt
        sentinel: $sentinel
      ) {
        noteId
      }
    }
  }
`;

export const UPDATE_SECURE_NOTE = gql`
  mutation UpdateSecureNote(
    $noteId: UUID!
    $category: String!
    $encryptedTitle: String!
    $encryptedContent: String!
    $iv: String!
  ) {
    fileManagement {
      updateSecureNote(
        noteId: $noteId
        category: $category
        encryptedTitle: $encryptedTitle
        encryptedContent: $encryptedContent
        iv: $iv
      ) {
        success
      }
    }
  }
`;

export const DELETE_SECURE_NOTE = gql`
  mutation DeleteSecureNote($noteId: UUID!) {
    fileManagement {
      deleteSecureNote(noteId: $noteId) {
        success
      }
    }
  }
`;
