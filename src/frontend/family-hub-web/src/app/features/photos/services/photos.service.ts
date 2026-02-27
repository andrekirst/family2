import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import {
  GET_PHOTOS,
  GET_PHOTO,
  GET_ADJACENT_PHOTOS,
  UPLOAD_PHOTO,
  UPDATE_PHOTO_CAPTION,
  DELETE_PHOTO,
} from '../graphql/photos.operations';
import { catchError, map, of } from 'rxjs';
import {
  PhotoDto,
  PhotosPageDto,
  AdjacentPhotosDto,
  UploadPhotoInput,
  UpdatePhotoCaptionInput,
} from '../models/photos.models';

@Injectable({
  providedIn: 'root',
})
export class PhotosService {
  private apollo = inject(Apollo);

  getPhotos(familyId: string, skip: number, take: number) {
    return this.apollo
      .query<{ family: { photos: PhotosPageDto } }>({
        query: GET_PHOTOS,
        variables: { familyId, skip, take },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map(
          (result) => result.data?.family?.photos ?? { items: [], totalCount: 0, hasMore: false },
        ),
        catchError((error) => {
          console.error('Failed to fetch photos:', error);
          return of({ items: [], totalCount: 0, hasMore: false } as PhotosPageDto);
        }),
      );
  }

  getPhoto(id: string) {
    return this.apollo
      .query<{ family: { photo: PhotoDto | null } }>({
        query: GET_PHOTO,
        variables: { id },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.family?.photo ?? null),
        catchError((error) => {
          console.error('Failed to fetch photo:', error);
          return of(null);
        }),
      );
  }

  getAdjacentPhotos(familyId: string, currentPhotoId: string, currentCreatedAt: string) {
    return this.apollo
      .query<{ family: { adjacentPhotos: AdjacentPhotosDto } }>({
        query: GET_ADJACENT_PHOTOS,
        variables: { familyId, currentPhotoId, currentCreatedAt },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map(
          (result) =>
            result.data?.family?.adjacentPhotos ??
            ({ previous: null, next: null } as AdjacentPhotosDto),
        ),
        catchError((error) => {
          console.error('Failed to fetch adjacent photos:', error);
          return of({ previous: null, next: null } as AdjacentPhotosDto);
        }),
      );
  }

  uploadPhoto(input: UploadPhotoInput) {
    return this.apollo
      .mutate<{ family: { photos: { upload: PhotoDto } } }>({
        mutation: UPLOAD_PHOTO,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.family?.photos?.upload ?? null),
        catchError((error) => {
          console.error('Failed to upload photo:', error);
          return of(null);
        }),
      );
  }

  updatePhotoCaption(id: string, input: UpdatePhotoCaptionInput) {
    return this.apollo
      .mutate<{ family: { photos: { updateCaption: PhotoDto } } }>({
        mutation: UPDATE_PHOTO_CAPTION,
        variables: { id, input },
      })
      .pipe(
        map((result) => result.data?.family?.photos?.updateCaption ?? null),
        catchError((error) => {
          console.error('Failed to update photo caption:', error);
          return of(null);
        }),
      );
  }

  deletePhoto(id: string) {
    return this.apollo
      .mutate<{ family: { photos: { delete: boolean } } }>({
        mutation: DELETE_PHOTO,
        variables: { id },
      })
      .pipe(
        map((result) => result.data?.family?.photos?.delete ?? false),
        catchError((error) => {
          console.error('Failed to delete photo:', error);
          return of(false);
        }),
      );
  }
}
