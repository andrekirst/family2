import { gql } from 'apollo-angular';

export const CREATE_FAMILY = gql`
  mutation CreateFamily($input: CreateFamilyRequestInput!) {
    createFamily(input: $input) {
      id
      name
      ownerId
      createdAt
      memberCount
    }
  }
`;
