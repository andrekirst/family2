import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import {
  ShareLinkDto,
  CreateShareLinkInput,
  ShareLinkAccessLogDto,
  FilePermissionDto,
  SetPermissionInput,
} from '../models/sharing.models';
import {
  GET_SHARE_LINKS,
  GET_SHARE_LINK_ACCESS_LOG,
  GET_PERMISSIONS,
  CREATE_SHARE_LINK,
  REVOKE_SHARE_LINK,
  SET_PERMISSION,
  REMOVE_PERMISSION,
} from '../graphql/sharing.operations';

@Injectable({ providedIn: 'root' })
export class SharingService {
  private readonly apollo = inject(Apollo);

  getShareLinks(familyId: string): Observable<ShareLinkDto[]> {
    return this.apollo
      .query<{ fileManagement: { shareLinks: ShareLinkDto[] } }>({
        query: GET_SHARE_LINKS,
        variables: { familyId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.shareLinks),
        catchError((err) => {
          console.error('Failed to load share links:', err);
          return of([]);
        }),
      );
  }

  getAccessLog(shareLinkId: string, familyId: string): Observable<ShareLinkAccessLogDto[]> {
    return this.apollo
      .query<{ fileManagement: { shareLinkAccessLog: ShareLinkAccessLogDto[] } }>({
        query: GET_SHARE_LINK_ACCESS_LOG,
        variables: { shareLinkId, familyId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.shareLinkAccessLog),
        catchError((err) => {
          console.error('Failed to load access log:', err);
          return of([]);
        }),
      );
  }

  getPermissions(resourceType: string, resourceId: string): Observable<FilePermissionDto[]> {
    return this.apollo
      .query<{ fileManagement: { permissions: FilePermissionDto[] } }>({
        query: GET_PERMISSIONS,
        variables: { resourceType, resourceId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.permissions),
        catchError((err) => {
          console.error('Failed to load permissions:', err);
          return of([]);
        }),
      );
  }

  createShareLink(
    input: CreateShareLinkInput,
  ): Observable<{ shareLinkId: string; token: string } | null> {
    return this.apollo
      .mutate<{
        fileManagement: {
          createShareLink: { success: boolean; shareLinkId: string; token: string };
        };
      }>({
        mutation: CREATE_SHARE_LINK,
        variables: input,
      })
      .pipe(
        map((r) => {
          const result = r.data?.fileManagement.createShareLink;
          return result?.success ? { shareLinkId: result.shareLinkId, token: result.token } : null;
        }),
        catchError((err) => {
          console.error('Failed to create share link:', err);
          return of(null);
        }),
      );
  }

  revokeShareLink(shareLinkId: string, familyId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { revokeShareLink: { success: boolean } } }>({
        mutation: REVOKE_SHARE_LINK,
        variables: { shareLinkId, familyId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.revokeShareLink.success ?? false),
        catchError((err) => {
          console.error('Failed to revoke share link:', err);
          return of(false);
        }),
      );
  }

  setPermission(input: SetPermissionInput): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { setPermission: { success: boolean } } }>({
        mutation: SET_PERMISSION,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.setPermission.success ?? false),
        catchError((err) => {
          console.error('Failed to set permission:', err);
          return of(false);
        }),
      );
  }

  removePermission(
    resourceType: string,
    resourceId: string,
    memberId: string,
  ): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { removePermission: { success: boolean } } }>({
        mutation: REMOVE_PERMISSION,
        variables: { resourceType, resourceId, memberId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.removePermission.success ?? false),
        catchError((err) => {
          console.error('Failed to remove permission:', err);
          return of(false);
        }),
      );
  }
}
