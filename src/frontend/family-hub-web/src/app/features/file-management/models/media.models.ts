export interface MediaStreamInfoDto {
  fileId: string;
  mimeType: string;
  fileSize: number;
  storageKey: string;
  supportsRangeRequests: boolean;
  isStreamable: boolean;
  thumbnails: FileThumbnailDto[];
}

export interface FileThumbnailDto {
  id: string;
  fileId: string;
  width: number;
  height: number;
  storageKey: string;
  generatedAt: string;
}
