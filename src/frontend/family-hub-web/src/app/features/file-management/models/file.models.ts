export interface StoredFileDto {
  id: string;
  name: string;
  mimeType: string;
  size: number;
  storageKey: string;
  checksum: string;
  folderId: string;
  familyId: string;
  uploadedBy: string;
  createdAt: string;
  updatedAt: string;
}

export interface UploadFileInput {
  name: string;
  mimeType: string;
  size: number;
  storageKey: string;
  checksum: string;
  folderId: string;
}

export interface UploadResult {
  storageKey: string;
  mimeType: string;
  size: number;
  checksum: string;
}

export interface RenameFileInput {
  fileId: string;
  newName: string;
}

export interface MoveFileInput {
  fileId: string;
  targetFolderId: string;
}
