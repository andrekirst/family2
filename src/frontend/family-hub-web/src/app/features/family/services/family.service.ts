import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { CREATE_FAMILY } from '../graphql/family.operations';
import { catchError, map, of } from 'rxjs';

export interface CreateFamilyInput {
  name: string;
}

export interface FamilyDto {
  id: string;
  name: string;
  owner: { id: string; name: string };
  createdAt: string;
  memberCount: number;
}

@Injectable({
  providedIn: 'root',
})
export class FamilyService {
  private apollo = inject(Apollo);

  createFamily(input: CreateFamilyInput) {
    return this.apollo
      .mutate<{ family: { create: FamilyDto } }>({
        mutation: CREATE_FAMILY,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.family?.create),
        catchError((error) => {
          console.error('Failed to create family:', error);
          return of(null);
        }),
      );
  }
}
