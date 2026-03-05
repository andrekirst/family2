import { gql } from 'apollo-angular';

const MESSAGE_FIELDS = `
  id
  familyId
  senderId
  senderName
  senderAvatarId
  content
  sentAt
  conversationId
  attachments {
    fileId
    fileName
    mimeType
    fileSize
    storageKey
    attachedAt
  }
`;

const CONVERSATION_MEMBER_FIELDS = `
  id
  userId
  role
  joinedAt
  leftAt
`;

const CONVERSATION_FIELDS = `
  id
  name
  type
  familyId
  createdBy
  folderId
  createdAt
  members {
    ${CONVERSATION_MEMBER_FIELDS}
  }
`;

export const GET_FAMILY_MESSAGES = gql`
  query GetFamilyMessages($limit: Int, $before: DateTime) {
    messaging {
      messages(limit: $limit, before: $before) {
        ${MESSAGE_FIELDS}
      }
    }
  }
`;

export const SEND_MESSAGE = gql`
  mutation SendMessage($input: SendMessageRequestInput!) {
    messaging {
      sendMessage(input: $input) {
        ${MESSAGE_FIELDS}
      }
    }
  }
`;

export const MESSAGE_SENT_SUBSCRIPTION = gql`
  subscription MessageSent($familyId: UUID!) {
    messageSent(familyId: $familyId) {
      ${MESSAGE_FIELDS}
    }
  }
`;

export const GET_CONVERSATIONS = gql`
  query GetConversations {
    messaging {
      conversations {
        ${CONVERSATION_FIELDS}
      }
    }
  }
`;

export const CREATE_CONVERSATION = gql`
  mutation CreateConversation($input: CreateConversationRequestInput!) {
    messaging {
      createConversation(input: $input) {
        ${CONVERSATION_FIELDS}
      }
    }
  }
`;

export const GET_CONVERSATION_MESSAGES = gql`
  query GetConversationMessages($conversationId: UUID!, $limit: Int, $before: DateTime) {
    messaging {
      conversationMessages(conversationId: $conversationId, limit: $limit, before: $before) {
        ${MESSAGE_FIELDS}
      }
    }
  }
`;
