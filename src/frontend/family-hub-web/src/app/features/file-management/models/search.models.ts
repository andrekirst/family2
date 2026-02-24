export interface FileSearchResultDto {
  id: string;
  name: string;
  mimeType: string;
  size: number;
  folderId: string;
  highlightedName: string | null;
  relevance: number | null;
  createdAt: string;
}

export interface SearchFilters {
  mimeTypes?: string[];
  dateFrom?: string;
  dateTo?: string;
  tagIds?: string[];
  folderId?: string;
}

export interface RecentSearchDto {
  id: string;
  query: string;
  searchedAt: string;
}

export interface SavedSearchDto {
  id: string;
  name: string;
  query: string;
  filtersJson: string | null;
  createdAt: string;
}

export interface SaveSearchInput {
  name: string;
  query: string;
  filtersJson?: string;
}
