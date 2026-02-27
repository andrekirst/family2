export type PhotoViewMode = 'grid' | 'viewer';

export interface PhotoDto {
  id: string;
  familyId: string;
  uploadedBy: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
  caption: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PhotosPageDto {
  items: PhotoDto[];
  totalCount: number;
  hasMore: boolean;
}

export interface AdjacentPhotosDto {
  previous: PhotoDto | null;
  next: PhotoDto | null;
}

export interface UploadPhotoInput {
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
  caption?: string | null;
}

export interface UpdatePhotoCaptionInput {
  caption?: string | null;
}

export const PHOTOS_CONSTANTS = {
  PAGE_SIZE: 30,
  GRID_COLS_SM: 2,
  GRID_COLS_MD: 3,
  GRID_COLS_LG: 4,
  GRID_COLS_XL: 5,
  GRID_COLS_2XL: 6,
} as const;
