import { gql } from '@apollo/client/core';

export const ME_QUERY = gql`
  query Me {
    me {
      id
      email
      emailVerified
      createdAt
    }
  }
`;
