import { gql } from 'apollo-angular';

export const GET_FAMILY_MESSAGES = gql`
  query GetFamilyMessages($limit: Int, $before: DateTime) {
    messaging {
      messages(limit: $limit, before: $before) {
        id
        familyId
        senderId
        senderName
        senderAvatarId
        content
        sentAt
      }
    }
  }
`;

export const SEND_MESSAGE = gql`
  mutation SendMessage($input: SendMessageRequestInput!) {
    messaging {
      sendMessage(input: $input) {
        id
        familyId
        senderId
        senderName
        senderAvatarId
        content
        sentAt
      }
    }
  }
`;

export const MESSAGE_SENT_SUBSCRIPTION = gql`
  subscription MessageSent($familyId: UUID!) {
    messageSent(familyId: $familyId) {
      id
      familyId
      senderId
      senderName
      senderAvatarId
      content
      sentAt
    }
  }
`;
