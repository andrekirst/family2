import { gql } from 'apollo-angular';

export const CREATE_FAMILY = gql`
  mutation CreateFamily($input: CreateFamilyInput!) {
    createFamily(input: $input) {
      id
      name
      ownerId
      createdAt
      memberCount
    }
  }
`;
