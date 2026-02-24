export interface FolderDto {
  id: string;
  name: string;
  parentFolderId: string | null;
  materializedPath: string;
  familyId: string;
  createdBy: string;
  createdAt: string;
}

export interface CreateFolderInput {
  name: string;
  parentFolderId?: string;
}

export interface RenameFolderInput {
  folderId: string;
  newName: string;
}

export interface MoveFolderInput {
  folderId: string;
  targetParentFolderId: string;
}
