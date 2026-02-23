export interface FileVersionDto {
  id: string;
  fileId: string;
  versionNumber: number;
  storageKey: string;
  fileSize: number;
  checksum: string;
  uploadedBy: string;
  isCurrent: boolean;
  uploadedAt: string;
}
