export interface TagDto {
  id: string;
  name: string;
  color: string;
  familyId: string;
  fileCount: number;
  createdAt: string;
}

export interface CreateTagInput {
  name: string;
  color: string;
}

export interface UpdateTagInput {
  tagId: string;
  name: string;
  color: string;
}
