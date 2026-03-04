import { gql } from 'apollo-angular';

const MESSAGE_FIELDS = `
  id
  familyId
  senderId
  senderName
  senderAvatarId
  content
  sentAt
  attachments {
    fileId
    fileName
    mimeType
    fileSize
    storageKey
    attachedAt
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
