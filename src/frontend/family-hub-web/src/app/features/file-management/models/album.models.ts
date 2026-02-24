export interface AlbumDto {
  id: string;
  name: string;
  description: string | null;
  coverFileId: string | null;
  familyId: string;
  createdBy: string;
  itemCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateAlbumInput {
  name: string;
  description?: string;
}
