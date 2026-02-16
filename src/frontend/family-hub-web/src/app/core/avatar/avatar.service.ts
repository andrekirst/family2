import { Injectable, inject } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { map, catchError, of, Observable } from 'rxjs';

const UPLOAD_AVATAR = gql`
  mutation UploadAvatar($input: UploadAvatarInput!) {
    family {
      uploadAvatar(input: $input) {
        avatarId
      }
    }
  }
`;

const REMOVE_AVATAR = gql`
  mutation RemoveAvatar {
    family {
      removeAvatar
    }
  }
`;

const SET_FAMILY_AVATAR = gql`
  mutation SetFamilyAvatar($input: SetFamilyAvatarInput!) {
    family {
      setFamilyAvatar(input: $input)
    }
  }
`;

export interface UploadAvatarInput {
  imageBase64: string;
  fileName: string;
  mimeType: string;
  cropX?: number;
  cropY?: number;
  cropWidth?: number;
  cropHeight?: number;
}

@Injectable({ providedIn: 'root' })
export class AvatarService {
  private apollo = inject(Apollo);

  uploadAvatar(input: UploadAvatarInput): Observable<string | null> {
    return this.apollo
      .mutate<{ family: { uploadAvatar: { avatarId: string } } }>({
        mutation: UPLOAD_AVATAR,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.family?.uploadAvatar?.avatarId ?? null),
        catchError((error) => {
          console.error('Failed to upload avatar:', error);
          return of(null);
        }),
      );
  }

  removeAvatar(): Observable<boolean> {
    return this.apollo
      .mutate<{ family: { removeAvatar: boolean } }>({
        mutation: REMOVE_AVATAR,
      })
      .pipe(
        map((result) => result.data?.family?.removeAvatar ?? false),
        catchError((error) => {
          console.error('Failed to remove avatar:', error);
          return of(false);
        }),
      );
  }

  setFamilyAvatar(avatarId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ family: { setFamilyAvatar: boolean } }>({
        mutation: SET_FAMILY_AVATAR,
        variables: { input: { avatarId } },
      })
      .pipe(
        map((result) => result.data?.family?.setFamilyAvatar ?? false),
        catchError((error) => {
          console.error('Failed to set family avatar:', error);
          return of(false);
        }),
      );
  }
}
