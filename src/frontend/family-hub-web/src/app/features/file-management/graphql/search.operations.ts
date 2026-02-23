import { gql } from 'apollo-angular';

export const SEARCH_FILES = gql`
  query SearchFiles(
    $query: String!
    $mimeTypes: [String!]
    $dateFrom: DateTime
    $dateTo: DateTime
    $tagIds: [UUID!]
    $folderId: UUID
    $sortBy: String = "relevance"
    $skip: Int = 0
    $take: Int = 20
  ) {
    fileManagement {
      searchFiles(
        query: $query
        mimeTypes: $mimeTypes
        dateFrom: $dateFrom
        dateTo: $dateTo
        tagIds: $tagIds
        folderId: $folderId
        sortBy: $sortBy
        skip: $skip
        take: $take
      ) {
        id
        name
        mimeType
        size
        folderId
        highlightedName
        relevance
        createdAt
      }
    }
  }
`;

export const GET_RECENT_SEARCHES = gql`
  query GetRecentSearches {
    fileManagement {
      getRecentSearches {
        id
        query
        createdAt
      }
    }
  }
`;

export const GET_SAVED_SEARCHES = gql`
  query GetSavedSearches {
    fileManagement {
      getSavedSearches {
        id
        query
        createdAt
      }
    }
  }
`;

export const SAVE_SEARCH = gql`
  mutation SaveSearch($query: String!, $filters: SearchFiltersDtoInput!) {
    fileManagement {
      saveSearch(query: $query, filters: $filters) {
        id
        query
        createdAt
      }
    }
  }
`;

export const DELETE_SAVED_SEARCH = gql`
  mutation DeleteSavedSearch($savedSearchId: UUID!) {
    fileManagement {
      deleteSavedSearch(savedSearchId: $savedSearchId)
    }
  }
`;
