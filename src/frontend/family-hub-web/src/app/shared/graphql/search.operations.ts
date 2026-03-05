import { gql } from 'apollo-angular';

export const UNIVERSAL_SEARCH_QUERY = gql`
  query UniversalSearch($input: UniversalSearchRequestInput!) {
    search {
      universal(input: $input) {
        results {
          title
          description
          module
          icon
          route
        }
        commands {
          label
          description
          keywords
          route
          requiredPermissions
          icon
          group
        }
      }
    }
  }
`;
