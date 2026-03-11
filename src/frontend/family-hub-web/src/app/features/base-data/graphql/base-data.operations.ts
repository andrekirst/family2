import { gql } from 'apollo-angular';

export const GET_FEDERAL_STATES = gql`
  query GetFederalStates {
    baseData {
      federalStates {
        id
        name
        iso3166Code
      }
    }
  }
`;

export const GET_FEDERAL_STATE_BY_ISO3166 = gql`
  query GetFederalStateByIso3166($code: String!) {
    baseData {
      federalStateByIso3166(code: $code) {
        id
        name
        iso3166Code
      }
    }
  }
`;
