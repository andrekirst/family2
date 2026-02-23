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

export interface SavedSearchDto {
  id: string;
  query: string;
  filters: SearchFilters;
  createdAt: string;
}

export interface SaveSearchInput {
  query: string;
  filters: SearchFilters;
}
