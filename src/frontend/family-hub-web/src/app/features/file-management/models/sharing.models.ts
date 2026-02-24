export interface ShareLinkDto {
  id: string;
  token: string;
  resourceType: 'File' | 'Folder';
  resourceId: string;
  familyId: string;
  createdBy: string;
  expiresAt: string | null;
  hasPassword: boolean;
  maxDownloads: number | null;
  downloadCount: number;
  isRevoked: boolean;
  isExpired: boolean;
  isAccessible: boolean;
  createdAt: string;
}

export interface CreateShareLinkInput {
  resourceType: 'File' | 'Folder';
  resourceId: string;
  familyId: string;
  expiresAt?: string;
  password?: string;
  maxDownloads?: number;
}

export interface ShareLinkAccessLogDto {
  id: string;
  shareLinkId: string;
  ipAddress: string;
  userAgent: string | null;
  action: 'View' | 'Download';
  accessedAt: string;
}

export interface FilePermissionDto {
  id: string;
  resourceType: 'File' | 'Folder';
  resourceId: string;
  memberId: string;
  permissionLevel: PermissionLevel;
  grantedBy: string;
  grantedAt: string;
}

export type PermissionLevel = 'View' | 'Edit' | 'Manage';

export interface SetPermissionInput {
  resourceType: 'file' | 'folder';
  resourceId: string;
  memberId: string;
  permissionLevel: number; // 1=View, 2=Edit, 3=Manage
}
