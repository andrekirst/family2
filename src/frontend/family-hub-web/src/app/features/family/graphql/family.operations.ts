import { gql } from 'apollo-angular';

export const CREATE_FAMILY = gql`
  mutation CreateFamily($input: CreateFamilyRequestInput!) {
    family {
      create(input: $input) {
        id
        name
        owner {
          id
          name
        }
        createdAt
        memberCount
      }
    }
  }
`;
